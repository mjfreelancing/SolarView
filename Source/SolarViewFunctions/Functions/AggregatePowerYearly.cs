using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarView.Common.Extensions;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Power;
using SolarViewFunctions.Repository.PowerYearly;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class AggregatePowerYearly : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public AggregatePowerYearly(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(AggregatePowerYearly))]
    public async Task Run([ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerTable,
      [Table(Constants.Table.PowerYearly, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable yearlyTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      Tracker.TrackEvent(nameof(AggregatePowerYearly));

      var request = context.GetInput<SiteRefreshAggregationRequest>();

      var startDate = request.StartDate.ParseSolarDate();    // expecting yyyy-MM-dd
      var endDate = request.EndDate.ParseSolarDate();
      var siteStartDate = request.SiteStartDate.ParseSolarDate();

      Tracker.TrackInfo($"Processing yearly aggregation for SiteId {request.SiteId}");    // don't log start/end date because it's not representative of what may be processed

      var powerRepository = _repositoryFactory.Create<IPowerRepository>(powerTable);
      var powerYearlyRepository = _repositoryFactory.Create<IPowerYearlyRepository>(yearlyTable);

      IEnumerable<Task> GetYearlyTasks()
      {
        for (var year = startDate.Year; year <= endDate.Year; year++)
        {
          var yearStartDate = new DateTime(year, 1, 1);
          var yearEndDate = new DateTime(year, 12, 31);

          // the first/last year may not be a complete year
          if (yearStartDate < siteStartDate)
          {
            yearStartDate = siteStartDate;
          }

          if (yearEndDate > endDate)
          {
            yearEndDate = endDate;
          }

          var daysToCollect = (yearEndDate - yearStartDate).Days + 1;

          foreach (var meterType in EnumHelper.GetEnumValues<MeterType>())
          {
            Tracker.TrackInfo($"Aggregating yearly {meterType} data for SiteId {request.SiteId}, ({yearStartDate.GetSolarDateString()} to {yearEndDate.GetSolarDateString()})");

            yield return PersistAggregatedMeterValues(powerRepository, powerYearlyRepository, request.SiteId, meterType, yearStartDate, daysToCollect);
          }
        }
      }

      var tasks = GetYearlyTasks();

      await Task.WhenAll(tasks).ConfigureAwait(false);

      Tracker.TrackInfo($"Yearly power data aggregation is complete for SiteId {request.SiteId}");
    }

    private static async Task PersistAggregatedMeterValues(IPowerRepository powerRepository, IPowerYearlyRepository powerYearlyRepository,
      string siteId, MeterType meterType, DateTime startDate, int daysToCollect)
    {
      var timeWatts = new Dictionary<string, (double Watts, double WattHour)>();

      for (var dayOffset = 0; dayOffset < daysToCollect; dayOffset++)
      {
        var date = startDate.AddDays(dayOffset);
        var meterEntities = powerRepository.GetMeterPowerAsyncEnumerable(siteId, date, meterType);

        await foreach (var entity in meterEntities)
        {
          // Note: can't seem to use TryGetValue() or GetValueOrDefault() with tuples without
          // complaining about possible null reference
          var (watts, wattHour) = (0.0d, 0.0d);

          if (timeWatts.ContainsKey(entity.Time))
          {
            (watts, wattHour) = timeWatts[entity.Time];
          }

          var totalWatts = watts + entity.Watts;
          var totalWattHour = wattHour + entity.WattHour;

          timeWatts[entity.Time] = (totalWatts, totalWattHour);
        }
      }

      // this will be prior to the actual last day of the week if it is a partial week
      var endDate = startDate.AddDays(daysToCollect - 1);

      var aggregatedEntities = timeWatts.Select(kvp =>
      {
        var time = kvp.Key;
        var (watts, wattHour) = kvp.Value;

        return new MeterPowerYearEntity(siteId, startDate, endDate, time, meterType, watts, wattHour);
      });

      await powerYearlyRepository.UpsertYearlyPowerAsync(aggregatedEntities).ConfigureAwait(false);
    }
  }
}