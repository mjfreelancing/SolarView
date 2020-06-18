using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarView.Common.Models;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Models;
using SolarViewFunctions.SolarEdge;
using SolarViewFunctions.Tracking;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class HydratePowerPeriodSubOrchestrator : FunctionBase
  {
    public HydratePowerPeriodSubOrchestrator(IRetryOptionsFactory retryOptionsFactory, ITracker tracker)
      : base(retryOptionsFactory, tracker)
    {
    }

    // Collects power data for a given date period
    // - can be used for initial bulk hydration
    // - can be used by a trigger function
    [FunctionName(nameof(HydratePowerPeriodSubOrchestrator))]
    public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      // allowing exceptions to bubble back to the caller

      MakeTrackerReplaySafe(context);
      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var powerQuery = context.GetInput<PowerQuery>();

      Tracker.TrackInfo(
        $"Received a request to orchestrate a power refresh for SiteId {powerQuery.SiteId} " +
        $"between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}"
      );

      // need to get the site API key
      var siteInfo = await context.CallActivityWithRetryAsync<SecretSiteInfo>(nameof(GetSiteInfo), GetDefaultRetryOptions(), powerQuery.SiteId);

      // executed via context.CallHttpAsync() - this method tracks the request
      var solarDataResponse = await SolarEdgeApi.GetSolarDataAsync(context, Constants.SolarEdge.MonitoringUri, siteInfo.ApiKey, powerQuery, Tracker);

      if (solarDataResponse.IsError)
      {
        // handled at the calling orchestrator
        throw new SolarEdgeResponseException(solarDataResponse.StatusCode, powerQuery.SiteId, powerQuery.StartDateTime, powerQuery.EndDateTime);
      }

      Tracker.TrackInfo($"Power data has been collected for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}");

      var siteData = new SiteSolarData(powerQuery.SiteId, powerQuery.StartDateTime, powerQuery.EndDateTime, solarDataResponse.SolarData);

      Tracker.TrackInfo($"Initiating a request to persist power data for SiteId {powerQuery.SiteId}");

      await context.CallActivityWithRetryAsync(nameof(PersistSolarDataAsPowerDocuments), GetDefaultRetryOptions(), siteData);
    }
  }
}