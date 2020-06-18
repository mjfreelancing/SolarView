using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Power;
using SolarViewFunctions.Repository.PowerMonthly;
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
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public AggregatePowerMonthly(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(AggregatePowerMonthly))]
    public async Task Run([ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerTable,
      [Table(Constants.Table.PowerMonthly, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable monthlyTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      Tracker.TrackEvent(nameof(AggregatePowerMonthly));

      var request = context.GetInput<SiteRefreshAggregationRequest>();

      var startDate = request.StartDate.ParseSolarDate();    // expecting yyyy-MM-dd
      var endDate = request.EndDate.ParseSolarDate();
      var siteStartDate = request.SiteStartDate.ParseSolarDate();

      Tracker.TrackInfo($"Processing monthly aggregation for SiteId {request.SiteId}");   // don't log start/end date because it's not representative of what may be processed

      var cultureInfo = new CultureInfo(Constants.AggregationOptions.CultureName);
      var calendar = cultureInfo.Calendar;

      var powerRepository = _repositoryFactory.Create<IPowerRepository>(powerTable);
      var powerMonthlyRepository = _repositoryFactory.Create<IPowerMonthlyRepository>(monthlyTable);

      IEnumerable<Task> GetMonthlyTasks()
      {
        for (var trackingStartDate = startDate.TrimToDayOfMonth(1);
          trackingStartDate <= endDate;
          trackingStartDate = trackingStartDate.AddMonths(1))
        {
          var monthStartDate = trackingStartDate.TrimToDayOfMonth(1);

          var lastDayInMonth = calendar.GetDaysInMonth(trackingStartDate.Year, trackingStartDate.Month);
          var monthEndDate = trackingStartDate.TrimToDayOfMonth(lastDayInMonth);

          // the first/last month may not be a complete month
          if (monthStartDate < siteStartDate)
          {
            monthStartDate = siteStartDate;
          }

          var daysToCollect = (monthEndDate - monthStartDate).Days + 1;

          foreach (var meterType in EnumHelper.GetEnumValues<MeterType>())
          {
            Tracker.TrackInfo($"Aggregating monthly {meterType} data for SiteId {request.SiteId}, ({monthStartDate.GetSolarDateString()} " +
                              $"to {monthEndDate.GetSolarDateString()})");

            yield return PersistAggregatedMeterValues(powerRepository, powerMonthlyRepository, request.SiteId, meterType, monthStartDate, daysToCollect);
          }
        }
      }

      var tasks = GetMonthlyTasks();

      await Task.WhenAll(tasks).ConfigureAwait(false);

      Tracker.TrackInfo($"Monthly power data aggregation is complete for SiteId {request.SiteId}");
    }

    private static async Task PersistAggregatedMeterValues(IPowerRepository powerRepository, IPowerMonthlyRepository powerMonthlyRepository, string siteId, MeterType meterType,
      DateTime startDate, int daysToCollect)
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
        return new MeterPowerMonth(siteId, startDate, endDate, time, meterType, watts);
      });

      await powerMonthlyRepository.UpsertAsync(aggregatedEntities).ConfigureAwait(false);
    }
  }
}