using AllOverIt.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class AggregatePowerData : FunctionBase
  {
    public AggregatePowerData(IRetryOptionsFactory retryOptionsFactory, ITracker tracker)
      : base(retryOptionsFactory, tracker)
    {
    }

    [FunctionName(nameof(AggregatePowerData))]
    public async Task Run(
      [OrchestrationTrigger] IDurableOrchestrationContext context,
      [Table(Constants.Table.Sites)] CloudTable sitesTable)
    {
      try
      {
        MakeTrackerReplaySafe(context);

        var currentTimeUtc = context.CurrentUtcDateTime;

        Tracker.TrackEvent(nameof(AggregatePowerData), new {context.InstanceId, RefreshTimeUtc = currentTimeUtc});

        var requests = GetDueSiteAggregationRequests(sitesTable, currentTimeUtc);

        if (requests.Count == 0)
        {
          Tracker.TrackInfo("No data aggregation to perform", new { context.InstanceId, RefreshTimeUtc = currentTimeUtc });
          return;
        }

        var tasks = requests.Select(request =>
        {
          Tracker.TrackInfo(
            $"Performing data aggregation for SiteId {request.SiteId}, DateTime {request.DateTime}",
            new { context.InstanceId, RefreshTimeUtc = currentTimeUtc }
          );

          return DoAggregations(context, request);
        });

        await Task.WhenAll(tasks);

        Tracker.TrackInfo("All data aggregation complete");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception.UnwrapFunctionException(), new {context.InstanceId});

        // todo: send a message to send an email
      }
    }

    private static IReadOnlyList<SiteRefreshAggregationRequest> GetDueSiteAggregationRequests(CloudTable sitesTable, DateTime currentTimeUtc)
    {
      var sitesDue = SitesHelpers.GetSites(
        sitesTable,
        site => site.UtcToLocalTime(currentTimeUtc).Hour == Constants.RefreshHour.Aggregation
      );

      return (
        from site in sitesDue
          let requestTimeUtc = currentTimeUtc.AddSeconds(-currentTimeUtc.Second)
          select new SiteRefreshAggregationRequest
          {
            SiteId = site.SiteId,
            DateTime = site.UtcToLocalTime(requestTimeUtc).GetSolarDateTimeString()
          }
        ).AsReadOnlyList();
    }

    private async Task DoAggregations(IDurableOrchestrationContext context, SiteRefreshAggregationRequest request)
    {
      Tracker.TrackInfo($"Requesting to aggregate weekly data for SiteId {request.SiteId}");
      await context.CallActivityWithRetryAsync(nameof(AggregatePowerWeekly), GetDefaultRetryOptions(), request);

      Tracker.TrackInfo($"Requesting to aggregate monthly data for SiteId {request.SiteId}");
      await context.CallActivityWithRetryAsync(nameof(AggregatePowerMonthly), GetDefaultRetryOptions(), request);

      Tracker.TrackInfo($"Requesting to aggregate yearly data for SiteId {request.SiteId}");
      await context.CallActivityWithRetryAsync(nameof(AggregatePowerYearly), GetDefaultRetryOptions(), request);
    }
  }
}