using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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
  public class AggregateSitePowerData : FunctionBase
  {
    private static readonly IDictionary<string, string> AggregateFunctions = new Dictionary<string, string>
    {
      {nameof(AggregatePowerMonthly), "monthly"},
      {nameof(AggregatePowerYearly), "yearly"}
    };

    private readonly IMapper _mapper;

    public AggregateSitePowerData(IRetryOptionsFactory retryOptionsFactory, ITracker tracker, IMapper mapper)
      : base(retryOptionsFactory, tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
    }

    [FunctionName(nameof(AggregateSitePowerData))]
    public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      // allowing exceptions to bubble back to the caller

      MakeTrackerReplaySafe(context);
      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var request = context.GetInput<SiteRefreshAggregationRequest>();

      Tracker.TrackEvent(nameof(AggregateSitePowerData), request);

      var tasks = GetAggregationTasks(context, request);

      await Task.WhenAll(tasks);

      if (request.TriggerType == RefreshTriggerType.Timed)
      {
        await UpdateLastAggregationEndDate(context, request.SiteId, request.EndDate);
      }
      else
      {
        Tracker.TrackInfo("Not updating last aggregation end date - the aggregation was triggered manually");
      }
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

        var aggregateRequest = _mapper.Map<SiteRefreshAggregationRequest>(request);
        aggregateRequest.StartDate = aggregateStartDate.GetSolarDateString();
        aggregateRequest.EndDate = aggregateEndDate.GetSolarDateString();

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
      Tracker.TrackInfo($"Updating last aggregation end date to {endDate}");

      var siteUpdates = new Dictionary<string, string>
      {
        {nameof(ISiteInfo.SiteId), siteId},
        {nameof(ISiteInfo.LastAggregationDate), endDate}
      };

      await context.CallActivityWithRetryAsync(nameof(UpdateSitesTable), GetDefaultRetryOptions(), siteUpdates);
    }
  }
}