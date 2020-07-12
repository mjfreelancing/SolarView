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

      // Note: if the two API calls below need to be run in parallel the calls will need to be moved to another function.
      // Not worth it in this case.

      // executed via context.CallHttpAsync() - this method tracks the request
      var powerDataResponse = await SolarEdgeApi.GetPowerDetailsAsync(context, Constants.SolarEdge.MonitoringUri, siteInfo.ApiKey, powerQuery, Tracker);

      if (powerDataResponse.IsError)
      {
        // handled at the calling orchestrator
        throw new SolarEdgeResponseException(powerDataResponse.StatusCode, powerQuery.SiteId, powerQuery.StartDateTime, powerQuery.EndDateTime);
      }

      var energyDataResponse = await SolarEdgeApi.GetEnergyDetailsAsync(context, Constants.SolarEdge.MonitoringUri, siteInfo.ApiKey, powerQuery, Tracker);

      if (energyDataResponse.IsError)
      {
        // handled at the calling orchestrator
        throw new SolarEdgeResponseException(energyDataResponse.StatusCode, powerQuery.SiteId, powerQuery.StartDateTime, powerQuery.EndDateTime);
      }

      var powerData = _mapper.Map<SolarData>(powerDataResponse.PowerData);
      var energyData = _mapper.Map<SolarData>(energyDataResponse.EnergyData);

      Tracker.TrackInfo($"Power and Energy data has been collected for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}");

      var solarDays = GetSolarViewDays(powerQuery.SiteId, powerData, energyData);

      var tasks = solarDays.Select(solarDay =>
      {
        Tracker.TrackInfo($"Initiating a request to persist power data for SiteId {solarDay.SiteId}, {solarDay.Date}");
        
        return context.CallActivityWithRetryAsync(nameof(PersistSolarViewDayAsMeterPoints), GetDefaultRetryOptions(), solarDay);
      });

      await Task.WhenAll(tasks);

      Tracker.TrackInfo($"All power data for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime} has been persisted");
    }

    private static IEnumerable<SolarViewDay> GetSolarViewDays(string siteId, SolarData powerData, SolarData energyData)
    {
      // flattened list of data points - so we can group into days and meter types
      var powerMeterPoints =
        from meter in powerData.MeterValues.Meters
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

      var energyMeterPoints =
        from meter in energyData.MeterValues.Meters
        let meterType = meter.Type.As<MeterType>()
        from value in meter.Values
        let timestamp = value.Date.ParseSolarDateTime()
        let wattHour = value.Value
        select new
        {
          timestamp.Date,
          MeterType = meterType,
          Timestamp = timestamp,
          WattHour = wattHour
        };

      var meterPoints = from power in powerMeterPoints
        join energy in energyMeterPoints
          on new {power.MeterType, power.Date, power.Timestamp}
          equals new {energy.MeterType, energy.Date, energy.Timestamp}
        select new
        {
          power.MeterType,
          power.Date,
          power.Timestamp,
          power.Watts,
          WattHour = energy.WattHour
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
                .Select(item => new SolarViewMeterPoint
                {
                  Timestamp = item.Timestamp,
                  Watts = item.Watts,
                  WattHour = item.WattHour
                })
            }
        };
    }
  }
}