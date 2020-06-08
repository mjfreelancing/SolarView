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
    public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      try
      {
        MakeTrackerReplaySafe(context);

        var refreshRequest = context.GetInput<SiteRefreshPowerRequest>();
        var siteLocalTime = refreshRequest.DateTime.ParseSolarDateTime();

        Tracker.TrackInfo($"Processing a {nameof(SiteRefreshPowerRequest)} message for SiteId {refreshRequest.SiteId}, DateTime {refreshRequest.DateTime}");

        var siteInfo = await context.CallActivityWithRetryAsync<SiteInfo>(nameof(GetSiteInfo), GetDefaultRetryOptions(), refreshRequest.SiteId);

        // next refresh time is in the site's timezone
        Tracker.TrackInfo($"Received info for SiteId {refreshRequest.SiteId}, next refresh due {siteInfo.NextRefreshDue}");

        await RefreshSite(context, siteInfo, siteLocalTime);
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception.UnwrapFunctionException(), new { context.InstanceId });

        // todo: send a message to send an email
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

      var powerQuery = new TriggeredPowerQuery
      {
        Trigger = RefreshTriggerType.Timed,
        SiteId = siteInfo.SiteId,
        StartDateTime = startDateTime.GetSolarDateTimeString(),
        EndDateTime = endDateTime.GetSolarDateTimeString()
      };

      await ProcessSitePowerQuery(context, powerQuery);
      await UpdateSitesRefreshTimes(context, siteInfo, powerQuery);
    }

    private Task ProcessSitePowerQuery(IDurableOrchestrationContext context, TriggeredPowerQuery powerQuery)
    {
      Tracker.TrackInfo(
        $"Initiating power hydration orchestration for SiteId {powerQuery.SiteId} is between " +
        $"{powerQuery.StartDateTime} and {powerQuery.EndDateTime}"
      );

      // fire off a request to (potentially) split the request into multiple date ranges
      return context.CallSubOrchestratorWithRetryAsync(nameof(HydratePowerOrchestrator), GetDefaultRetryOptions(), powerQuery);
    }

    private Task UpdateSitesRefreshTimes(IDurableOrchestrationContext context, SiteInfo siteInfo, PowerQuery powerQuery)
    {
      Tracker.TrackInfo($"Initiating a request to update SiteId {powerQuery.SiteId} with a last refresh date of {powerQuery.EndDateTime}");

      // update the refresh timestamps for this site - will only occur of there are no errors.
      // if there is an error then the request will be re-processed when the trigger is next fired.
      siteInfo.UpdateRefreshTimes(powerQuery.EndDateTime);
      return context.CallActivityWithRetryAsync(nameof(UpdateSitesTable), GetDefaultRetryOptions(), siteInfo);
    }
  }
}