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
        Tracker.AppendDefaultProperties(context.GetTrackingProperties());

        var currentTimeUtc = context.CurrentUtcDateTime;

        Tracker.TrackEvent(nameof(AggregatePowerData));

        var requests = GetSitesDueForAggregationRequests(sitesTable, currentTimeUtc);

        if (requests.Count == 0)
        {
          Tracker.TrackInfo("No data aggregation to perform");
          return;
        }

        var tasks = requests.Select(request =>
        {
          Tracker.TrackInfo($"Initiating data aggregation for SiteId {request.SiteId} between {request.StartDate} and {request.EndDate}");

          return context.CallSubOrchestratorWithRetryAsync(nameof(AggregateSitePowerData), GetDefaultRetryOptions(), request);
        });

        await Task.WhenAll(tasks);

        Tracker.TrackInfo("All data aggregation complete");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception.UnwrapFunctionException());

        // todo: send a message to send an email
      }
    }

    private static IReadOnlyList<SiteRefreshAggregationRequest> GetSitesDueForAggregationRequests(CloudTable sitesTable, DateTime currentTimeUtc)
    {
      var sitesDue = SitesHelpers.GetSites(
        sitesTable,
        site => site.UtcToLocalTime(currentTimeUtc).Hour == Constants.RefreshHour.Aggregation
      );

      // the request date will be the day prior to the current time (to ensure only full days are processed)
      return (
        from site in sitesDue
        let nextAggregation = site.GetNextAggregationPeriod(site.UtcToLocalTime(currentTimeUtc).Date)
        where nextAggregation.EndDate > nextAggregation.StartDate
        select new SiteRefreshAggregationRequest
        {
          SiteId = site.SiteId,
          StartDate = nextAggregation.StartDate.GetSolarDateString(),
          EndDate = nextAggregation.EndDate.GetSolarDateString()
        }
      ).AsReadOnlyList();
    }
  }
}