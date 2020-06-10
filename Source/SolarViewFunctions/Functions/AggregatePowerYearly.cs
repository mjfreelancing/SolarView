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
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class AggregatePowerYearly : FunctionBase
  {
    public AggregatePowerYearly(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(AggregatePowerYearly))]
    public async Task Run([ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power)] CloudTable powerTable,
      [Table(Constants.Table.PowerYearly)] CloudTable yearlyTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      Tracker.TrackEvent(nameof(AggregatePowerYearly));

      var request = context.GetInput<SiteRefreshAggregationRequest>();

      var startDate = request.StartDate.ParseSolarDate();    // expecting yyyy-MM-dd
      var endDate = request.EndDate.ParseSolarDate();

      Tracker.TrackInfo($"Processing yearly aggregation for SiteId {request.SiteId} between {startDate.GetSolarDateTimeString()} and {endDate.GetSolarDateTimeString()}");

      IEnumerable<Task> GetYearlyTasks()
      {
        for (var year = startDate.Year; year <= endDate.Year; year++)
        {
          var yearStartDate = new DateTime(year, 1, 1);
          var yearEndDate = new DateTime(year, 12, 31);

          // the first/last year may not be a complete year
          if (yearStartDate < startDate)
          {
            yearStartDate = startDate;
          }

          if (yearEndDate > endDate)
          {
            yearEndDate = endDate;
          }

          var daysToCollect = (yearEndDate - yearStartDate).Days + 1;

          foreach (var meterType in EnumHelper.GetEnumValues<MeterType>())
          {
            Tracker.TrackInfo($"Aggregating {meterType} data for SiteId {request.SiteId}, ({yearStartDate.GetSolarDateString()} to {yearEndDate.GetSolarDateString()})");

            yield return PersistAggregatedMeterValues(powerTable, yearlyTable, request.SiteId, meterType, yearStartDate, daysToCollect);
          }
        }
      }

      var tasks = GetYearlyTasks();

      await Task.WhenAll(tasks);

      Tracker.TrackInfo($"Yearly power data aggregation is complete for SiteId {request.SiteId}");
    }

    private static Task PersistAggregatedMeterValues(CloudTable powerTable, CloudTable yearlyTable, string siteId, MeterType meterType,
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
          return new MeterPowerYear(siteId, startDate, endDate, time, meterType, watts);
        });

      return yearlyTable.BatchInsertOrReplaceAsync(aggregatedEntities);
    }
  }
}