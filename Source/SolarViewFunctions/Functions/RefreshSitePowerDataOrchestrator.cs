using AllOverIt.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;
using System;
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
    public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      MakeTrackerReplaySafe(context);
      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      SiteRefreshPowerRequest request = null;

      try
      {
        request = context.GetInput<SiteRefreshPowerRequest>();
        var siteLocalTime = request.DateTime.ParseSolarDateTime();

        Tracker.TrackInfo($"Processing a {nameof(SiteRefreshPowerRequest)} message for SiteId {request.SiteId}, DateTime {request.DateTime}");

        var siteInfo = await context.CallActivityWithRetryAsync<SiteInfo>(nameof(GetSiteInfo), GetDefaultRetryOptions(), request.SiteId);

        // next refresh time is in the site's timezone
        Tracker.TrackInfo(siteInfo.LastRefreshDateTime.IsNullOrEmpty()
          ? $"Received info for SiteId {request.SiteId}, pending initial refresh"
          : $"Received info for SiteId {request.SiteId}, last refresh was {siteInfo.LastRefreshDateTime}");

        await RefreshSite(context, siteInfo, siteLocalTime);
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
          await exceptionDocuments.AddNotificationAsync<RefreshSitePowerDataOrchestrator>(request.SiteId, exception, notification);
        }
      }
    }

    private async Task RefreshSite(IDurableOrchestrationContext context, SiteInfo siteInfo, DateTime siteLocalTime)
    {
      var (startDateTime, endDateTime) = siteInfo.GetNextRefreshPeriod(siteLocalTime);

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

      await ProcessSitePowerQuery(context, triggeredPowerQuery);
      await UpdateSiteLastRefreshTime(context, siteInfo, triggeredPowerQuery);
    }

    private Task ProcessSitePowerQuery(IDurableOrchestrationContext context, PowerQuery powerQuery)
    {
      Tracker.TrackInfo(
        $"Initiating power hydration orchestration for SiteId {powerQuery.SiteId} is between " +
        $"{powerQuery.StartDateTime} and {powerQuery.EndDateTime}"
      );

      // fire off a request to (potentially) split the request into multiple date ranges
      return context.CallSubOrchestratorWithRetryAsync(nameof(HydratePowerOrchestrator), GetDefaultRetryOptions(), powerQuery);
    }

    private Task UpdateSiteLastRefreshTime(IDurableOrchestrationContext context, SiteInfo siteInfo, PowerQuery powerQuery)
    {
      Tracker.TrackInfo($"Initiating a request to update SiteId {powerQuery.SiteId} with a last refresh date of {powerQuery.EndDateTime}");

      siteInfo.LastRefreshDateTime = powerQuery.EndDateTime;
      return context.CallActivityWithRetryAsync(nameof(UpdateSitesTable), GetDefaultRetryOptions(), siteInfo);
    }
  }
}