using AllOverIt.Extensions;
using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarView.Common.Extensions;
using SolarView.Common.Models;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Models;
using SolarViewFunctions.Models.SolarEdgeData;
using SolarViewFunctions.SolarEdge;
using SolarViewFunctions.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class HydratePowerPeriodSubOrchestrator : FunctionBase
  {
    private readonly IMapper _mapper;

    public HydratePowerPeriodSubOrchestrator(IRetryOptionsFactory retryOptionsFactory, ITracker tracker, IMapper mapper)
      : base(retryOptionsFactory, tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
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

      var solarData = _mapper.Map<SolarData>(solarDataResponse.SolarData);

      Tracker.TrackInfo($"Power data has been collected for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}");

      var solarDays = GetSolarViewDays(powerQuery.SiteId, solarData);

      var tasks = solarDays.Select(solarDay =>
      {
        Tracker.TrackInfo($"Initiating a request to persist power data for SiteId {solarDay.SiteId}, {solarDay.Date}");
        
        return context.CallActivityWithRetryAsync(nameof(SolarViewDayAsMeterPoints), GetDefaultRetryOptions(), solarDay);
      });

      await Task.WhenAll(tasks);

      Tracker.TrackInfo($"All power data for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime} has been persisted");
    }

    private static IEnumerable<SolarViewDay> GetSolarViewDays(string siteId, SolarData solarData)
    {
      // flattened list of data points - so we can group into days and meter types
      var meterPoints =
        from meter in solarData.MeterValues.Meters
        let meterType = meter.Type.As<MeterType>()
        from value in meter.Values
        let timestamp = value.Date.ParseSolarDateTime()
        let watts = value.Value
        select new
        {
          timestamp.Date,
          MeterType = meterType,
          Timestamp = timestamp,
          Watts = watts
        };

      return
        from dailyMeterPoints in meterPoints.GroupBy(item => item.Date)
        select new SolarViewDay
        {
          SiteId = siteId,
          Date = dailyMeterPoints.Key.GetSolarDateString(),
          Meters =
            from dailyMeterPoint in dailyMeterPoints.GroupBy(item => item.MeterType)
            select new SolarViewMeter
            {
              MeterType = dailyMeterPoint.Key,
              Points = dailyMeterPoint
                .OrderBy(item => item.Timestamp)
                .Select(item =>
                  new SolarViewMeterPoint
                  {
                    Timestamp = item.Timestamp,
                    Watts = item.Watts
                  })
            }
        };
    }
  }
}