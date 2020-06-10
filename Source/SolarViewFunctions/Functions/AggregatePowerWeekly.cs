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
  public class AggregatePowerWeekly : FunctionBase
  {
    public AggregatePowerWeekly(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(AggregatePowerWeekly))]
    public async Task Run([ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power)] CloudTable powerTable,
      [Table(Constants.Table.PowerWeekly)] CloudTable weeklyTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      Tracker.TrackEvent(nameof(AggregatePowerWeekly));

      var request = context.GetInput<SiteRefreshAggregationRequest>();

      var startDate = request.StartDate.ParseSolarDate();    // expecting yyyy-MM-dd
      var endDate = request.EndDate.ParseSolarDate();

      Tracker.TrackInfo($"Processing weekly aggregation for SiteId {request.SiteId} between {startDate.GetSolarDateTimeString()} and {endDate.GetSolarDateTimeString()}");

      var (firstWeekNumber, firstDateOfFirstWeek) = GetWeekOfYear(startDate);
      var (lastWeekNumber, _) = GetWeekOfYear(endDate);

      IEnumerable<Task> GetWeeklyTasks()
      {
        for (var weekNumber = firstWeekNumber; weekNumber <= lastWeekNumber; weekNumber++)
        {
          var weekStartDate = firstDateOfFirstWeek.AddDays(7 * (weekNumber - firstWeekNumber));
          var weekEndDate = weekStartDate.AddDays(6);

          // the first/last week may not be a complete week
          if (weekStartDate < startDate)
          {
            weekStartDate = startDate;
          }

          if (weekEndDate > endDate)
          {
            weekEndDate = endDate;
          }

          var daysToCollect = (weekEndDate - weekStartDate).Days + 1;

          foreach (var meterType in EnumHelper.GetEnumValues<MeterType>())
          {
            Tracker.TrackInfo(
              $"Aggregating {meterType} data for SiteId {request.SiteId}, Week {weekNumber} " +
              $"({weekStartDate.GetSolarDateString()} to {weekEndDate.GetSolarDateString()})"
            );

            yield return PersistAggregatedMeterValues(powerTable, weeklyTable, request.SiteId, meterType, weekNumber, weekStartDate, daysToCollect);
          }
        }
      }

      var tasks = GetWeeklyTasks();

      await Task.WhenAll(tasks);

      Tracker.TrackInfo($"Weekly power data aggregation is complete for SiteId {request.SiteId}");
    }

    private static Task PersistAggregatedMeterValues(CloudTable powerTable, CloudTable weeklyTable, string siteId, MeterType meterType,
      int weekNumber, DateTime startDate, int daysToCollect)
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
        return new MeterPowerWeek(siteId, startDate, endDate, time, weekNumber, meterType, watts);
      });

      return weeklyTable.BatchInsertOrReplaceAsync(aggregatedEntities);
    }

    private static (int weekNumber, DateTime firstDateOfWeek) GetWeekOfYear(DateTime dateTime)
    {
      var cultureInfo = new CultureInfo(Constants.AggregationOptions.CultureName);
      var calendar = cultureInfo.Calendar;
      var weekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;
      var firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
    
      var weekNumber = calendar.GetWeekOfYear(dateTime, weekRule, firstDayOfWeek);
      var firstDateOfWeek = GetFirstDateOfWeek(cultureInfo, dateTime);

      return (weekNumber, firstDateOfWeek);
    }

    private static DateTime GetFirstDateOfWeek(CultureInfo cultureInfo, DateTime dateTime)
    {
      var first = (int)cultureInfo.DateTimeFormat.FirstDayOfWeek;
      var current = (int)dateTime.DayOfWeek;

      return first <= current
        ? dateTime.AddDays(-1 * (current - first))
        : dateTime.AddDays(first - current - 7);
    }
  }
}