using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Power;
using SolarViewFunctions.Repository.PowerWeekly;
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
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public AggregatePowerWeekly(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(AggregatePowerWeekly))]
    public async Task Run([ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerTable,
      [Table(Constants.Table.PowerWeekly, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable weeklyTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      Tracker.TrackEvent(nameof(AggregatePowerWeekly));

      var request = context.GetInput<SiteRefreshAggregationRequest>();

      var startDate = request.StartDate.ParseSolarDate();
      var endDate = request.EndDate.ParseSolarDate();
      var siteStartDate = request.SiteStartDate.ParseSolarDate();

      Tracker.TrackInfo($"Processing weekly aggregation for SiteId {request.SiteId}");    // don't log start/end date because it's not representative of what may be processed

      var (firstWeekNumber, firstDateOfFirstWeek) = GetWeekOfYear(startDate);
      var (lastWeekNumber, _) = GetWeekOfYear(endDate);

      var powerRepository = _repositoryFactory.Create<IPowerRepository>(powerTable);
      var powerWeeklyRepository = _repositoryFactory.Create<IPowerWeeklyRepository>(weeklyTable);

      IEnumerable<Task> GetWeeklyTasks()
      {
        for (var weekNumber = firstWeekNumber; weekNumber <= lastWeekNumber; weekNumber++)
        {
          var weekStartDate = firstDateOfFirstWeek.AddDays(7 * (weekNumber - firstWeekNumber));
          var weekEndDate = weekStartDate.AddDays(6);

          // the first/last week may not be a complete week
          if (weekStartDate < siteStartDate)
          {
            weekStartDate = siteStartDate;
          }

          if (weekEndDate > endDate)
          {
            weekEndDate = endDate;
          }

          var daysToCollect = (weekEndDate - weekStartDate).Days + 1;

          foreach (var meterType in EnumHelper.GetEnumValues<MeterType>())
          {
            Tracker.TrackInfo(
              $"Aggregating weekly {meterType} data for SiteId {request.SiteId}, Week {weekNumber} " +
              $"({weekStartDate.GetSolarDateString()} to {weekEndDate.GetSolarDateString()})"
            );

            yield return PersistAggregatedMeterValues(powerRepository, powerWeeklyRepository, request.SiteId, meterType, weekNumber, weekStartDate, daysToCollect);
          }
        }
      }

      var tasks = GetWeeklyTasks();

      await Task.WhenAll(tasks).ConfigureAwait(false);

      Tracker.TrackInfo($"Weekly power data aggregation is complete for SiteId {request.SiteId}");
    }

    private static async Task PersistAggregatedMeterValues(IPowerRepository powerRepository, IPowerWeeklyRepository powerWeeklyRepository,
      string siteId, MeterType meterType, int weekNumber, DateTime startDate, int daysToCollect)
    {
      var timeWatts = new Dictionary<string, double>();

      for (var dayOffset = 0; dayOffset < daysToCollect; dayOffset++)
      {
        var date = startDate.AddDays(dayOffset);
        var meterEntities = powerRepository.GetMeterPowerAsyncEnumerable(siteId, date, meterType);

        await foreach (var entity in meterEntities)
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

      await powerWeeklyRepository.UpsertAsync(aggregatedEntities).ConfigureAwait(false);
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