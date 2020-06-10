using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class AggregatePowerMonthly : FunctionBase
  {
    public AggregatePowerMonthly(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(AggregatePowerMonthly))]
    public async Task Run([ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power)] CloudTable powerTable,
      [Table(Constants.Table.PowerMonthly)] CloudTable monthlyTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      Tracker.TrackEvent(nameof(AggregatePowerMonthly));

      var request = context.GetInput<SiteRefreshAggregationRequest>();

      var startDate = request.StartDate.ParseSolarDate();    // expecting yyyy-MM-dd
      var endDate = request.EndDate.ParseSolarDate();

      Tracker.TrackInfo($"Processing monthly aggregation for SiteId {request.SiteId} between {startDate.GetSolarDateTimeString()} and {endDate.GetSolarDateTimeString()}");

      var cultureInfo = new CultureInfo(Constants.AggregationOptions.CultureName);
      var calendar = cultureInfo.Calendar;

      IEnumerable<Task> GetMonthlyTasks()
      {
        for (var trackingStartDate = new DateTime(startDate.Year, startDate.Month, 1);
          trackingStartDate <= endDate;
          trackingStartDate = trackingStartDate.AddMonths(1))
        {
          var monthStartDate = new DateTime(trackingStartDate.Year, trackingStartDate.Month, 1);

          var lastDayInMonth = calendar.GetDaysInMonth(trackingStartDate.Year, trackingStartDate.Month);
          var monthEndDate = new DateTime(trackingStartDate.Year, trackingStartDate.Month, lastDayInMonth);

          // the first/last month may not be a complete month
          if (monthStartDate < startDate)
          {
            monthStartDate = startDate;
          }

          if (monthEndDate > endDate)
          {
            monthEndDate = endDate;
          }

          var daysToCollect = (monthEndDate - monthStartDate).Days + 1;

          foreach (var meterType in EnumHelper.GetEnumValues<MeterType>())
          {
            Tracker.TrackInfo(
              $"Aggregating {meterType} data for SiteId {request.SiteId}, ({monthStartDate.GetSolarDateString()} to {monthEndDate.GetSolarDateString()})"
            );

            yield return PersistAggregatedMeterValues(powerTable, monthlyTable, request.SiteId, meterType, monthStartDate, daysToCollect);
          }
        }
      }

      var tasks = GetMonthlyTasks();

      await Task.WhenAll(tasks);

      Tracker.TrackInfo($"Monthly power data aggregation is complete for SiteId {request.SiteId}");
    }

    private static Task PersistAggregatedMeterValues(CloudTable powerTable, CloudTable monthlyTable, string siteId, MeterType meterType,
      DateTime startDate, int daysToCollect)
    {
      var timeWatts = new Dictionary<string, double>();

      for (var dayOffset = 0; dayOffset < daysToCollect; dayOffset++)
      {
        var date = startDate.AddDays(dayOffset);
        var partitionKey = $"{siteId}_{date:yyyyMMdd}_{meterType}";

        var entities = powerTable.GetPartitionItems<MeterPower>(partitionKey);

        foreach (var entity in entities)
        {
          var totalWatts = timeWatts.GetValueOrDefault(entity.Time) + entity.Watts;
          timeWatts[entity.Time] = totalWatts;
        }
      }

      // this will be prior to the actual last day of the week if it is a partial week
      var endDate = startDate.AddDays(daysToCollect - 1);

      var aggregatedEntities = timeWatts.Select(kvp =>
      {
        var (time, watts) = kvp;
        return new MeterPowerMonth(siteId, startDate, endDate, time, meterType, watts);
      });

      return monthlyTable.BatchInsertOrReplaceAsync(aggregatedEntities);
    }
  }
}