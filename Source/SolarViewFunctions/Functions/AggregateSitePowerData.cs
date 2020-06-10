using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class AggregateSitePowerData : FunctionBase
  {
    private static readonly IDictionary<string, string> AggregateFunctions = new Dictionary<string, string>
    {
      {nameof(AggregatePowerWeekly), "weekly"},
      {nameof(AggregatePowerMonthly), "monthly"},
      {nameof(AggregatePowerYearly), "yearly"}
    };

    public AggregateSitePowerData(IRetryOptionsFactory retryOptionsFactory, ITracker tracker)
      : base(retryOptionsFactory, tracker)
    {
    }

    [FunctionName(nameof(AggregateSitePowerData))]
    public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      // allowing exceptions to bubble back to the caller

      MakeTrackerReplaySafe(context);
      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      Tracker.TrackEvent(nameof(AggregateSitePowerData));

      var request = context.GetInput<SiteRefreshAggregationRequest>();

      var tasks = GetAggregationTasks(context, request);

      await Task.WhenAll(tasks);

      await UpdateLastAggregationEndDate(context, request.SiteId, request.EndDate);
    }

    private IEnumerable<Task> GetAggregationTasks(IDurableOrchestrationContext context, SiteRefreshAggregationRequest request)
    {
      // check if the start and end date span different years
      var startDate = request.StartDate.ParseSolarDate();
      var endDate = request.EndDate.ParseSolarDate();

      // we could compare (startDate.Year == endDate.Year) and yield GetAggregationBatch(context, request) when they are the same
      // but the overhead in the loop below is minimal enough not to be concerned. The loop is applicable for initial data population
      // and then as each year ticks over.
      for (var year = startDate.Year; year <= endDate.Year; year++)
      {
        var aggregateStartDate = year == startDate.Year ? startDate : new DateTime(year, 1, 1);
        var aggregateEndDate = year == endDate.Year ? endDate : new DateTime(year, 12, 31);

        var aggregateRequest = new SiteRefreshAggregationRequest
        {
          SiteId = request.SiteId,
          StartDate = aggregateStartDate.GetSolarDateString(),
          EndDate = aggregateEndDate.GetSolarDateString()
        };

        yield return GetAggregationBatch(context, aggregateRequest);
      }
    }

    private async Task GetAggregationBatch(IDurableOrchestrationContext context, SiteRefreshAggregationRequest request)
    {
      // these functions MUST be called in sequence
      foreach (var (functionName, frequency) in AggregateFunctions)
      {
        Tracker.TrackInfo($"Requesting to aggregate {frequency} data for SiteId {request.SiteId} between {request.StartDate} and {request.EndDate}");

        await context.CallActivityWithRetryAsync(functionName, GetDefaultRetryOptions(), request);
      }
    }

    private async Task UpdateLastAggregationEndDate(IDurableOrchestrationContext context, string siteId, string endDate)
    {
      var siteInfo = await context.CallActivityWithRetryAsync<SiteInfo>(nameof(GetSiteInfo), GetDefaultRetryOptions(), siteId);

      Tracker.TrackInfo($"Updating last aggregation end date to {endDate}");
      siteInfo.LastAggregationDate = endDate;

      await context.CallActivityWithRetryAsync(nameof(UpdateSitesTable), GetDefaultRetryOptions(), siteInfo);
    }
  }
}