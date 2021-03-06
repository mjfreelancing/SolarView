using AllOverIt.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarView.Common.Extensions;
using SolarView.Common.Models;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class RefreshSitePowerDataOrchestrator : FunctionBase
  {
    public RefreshSitePowerDataOrchestrator(IRetryOptionsFactory retryOptionsFactory, ITracker tracker)
      : base(retryOptionsFactory, tracker)
    {
    }

    [FunctionName(nameof(RefreshSitePowerDataOrchestrator))]
    public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      MakeTrackerReplaySafe(context);
      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      SiteRefreshPowerRequest request = null;

      try
      {
        request = context.GetInput<SiteRefreshPowerRequest>();

        Tracker.TrackInfo($"Processing a {nameof(SiteRefreshPowerRequest)} message for SiteId {request.SiteId}, between {request.StartDateTime} and {request.EndDateTime}");

        var siteInfo = await context.CallActivityWithRetryAsync<SecretSiteInfo>(nameof(GetSiteInfo), GetDefaultRetryOptions(), request.SiteId);

        // next refresh time is in the site's timezone
        Tracker.TrackInfo(siteInfo.LastRefreshDateTime.IsNullOrEmpty()
          ? $"Received info for SiteId {request.SiteId}, pending initial refresh"
          : $"Received info for SiteId {request.SiteId}, last refresh was {siteInfo.LastRefreshDateTime}");

        await RefreshSite(context, siteInfo, request.StartDateTime.ParseSolarDateTime(), request.EndDateTime.ParseSolarDateTime());
      }
      catch (Exception exception)
      {
        var trackedException = exception.UnwrapFunctionException();

        Tracker.TrackException(trackedException);

        var notification = new
        {
          request?.SiteId,
          ContextInput = request
        };

        Tracker.TrackException(exception, notification);

        if (!request?.SiteId.IsNullOrEmpty() ?? false)
        {
          // can't use I/O in an orchestration context so need to indirectly report the problem via another activity
          await context.NotifyException<RefreshSitePowerDataOrchestrator>(GetDefaultRetryOptions(), request.SiteId, trackedException, notification);
        }
      }
    }

    private async Task RefreshSite(IDurableOrchestrationContext context, SecretSiteInfo siteInfo, DateTime startDateTime, DateTime endDateTime)
    {
      // ignore any message received that is earlier than the site's 'LastRefreshEnd'
      if (endDateTime < startDateTime)
      {
        Tracker.TrackWarn(
          $"Ignoring a request received for {endDateTime.GetSolarDateTimeString()} that is older than " +
          $"the last known refresh time {startDateTime.GetSolarDateTimeString()}"
        );

        return;
      }

      Tracker.TrackInfo(
        $"Determined the next refresh period for SiteId {siteInfo.SiteId} is between " +
        $"{startDateTime.GetSolarDateTimeString()} and {endDateTime.GetSolarDateTimeString()}"
      );

      var triggeredPowerQuery = new TriggeredPowerQuery(siteInfo.UtcToLocalTime(context.CurrentUtcDateTime), RefreshTriggerType.Timed)
      {
        SiteId = siteInfo.SiteId,
        StartDateTime = startDateTime.GetSolarDateTimeString(),
        EndDateTime = endDateTime.GetSolarDateTimeString()
      };

      var status = await ProcessSitePowerQuery(context, triggeredPowerQuery);

      if (status == PowerUpdatedStatus.Completed)
      {
        siteInfo.LastRefreshDateTime = triggeredPowerQuery.EndDateTime;
        await UpdateSiteLastRefreshTime(context, siteInfo);
      }
      else
      {
        Tracker.TrackWarn($"Power refresh for SiteId {siteInfo.SiteId} failed. Not updating last refresh timestamp.");
      }
    }

    private Task<PowerUpdatedStatus> ProcessSitePowerQuery(IDurableOrchestrationContext context, PowerQuery powerQuery)
    {
      Tracker.TrackInfo(
        $"Initiating power hydration orchestration for SiteId {powerQuery.SiteId} is between " +
        $"{powerQuery.StartDateTime} and {powerQuery.EndDateTime}"
      );

      // fire off a request to (potentially) split the request into multiple date ranges
      return context.CallSubOrchestratorWithRetryAsync<PowerUpdatedStatus>(nameof(HydratePowerOrchestrator), GetDefaultRetryOptions(), powerQuery);
    }

    private Task UpdateSiteLastRefreshTime(IDurableOrchestrationContext context, ISiteInfo siteInfo)
    {
      Tracker.TrackInfo($"Initiating a request to update SiteId {siteInfo.SiteId} with a last refresh date of {siteInfo.LastRefreshDateTime}");

      var siteUpdates = new Dictionary<string, string>
      {
        {nameof(ISiteInfo.SiteId), siteInfo.SiteId},
        {nameof(ISiteInfo.LastRefreshDateTime), siteInfo.LastRefreshDateTime}
      };

      return context.CallActivityWithRetryAsync(nameof(UpdateSitesTable), GetDefaultRetryOptions(), siteUpdates);
    }
  }
}