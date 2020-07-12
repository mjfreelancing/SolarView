using AllOverIt.Extensions;
using AllOverIt.Helpers;
using AllOverIt.Tasks;
using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Power;
using SolarViewFunctions.Repository.PowerMonthly;
using SolarViewFunctions.Repository.PowerYearly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Providers
{
  public class PowerAggregationProvider : IPowerAggregationProvider
  {
    private static readonly DateRange ExcludedNoDates = new DateRange(DateTime.MaxValue, DateTime.MinValue);

    // creates a new instance - avoid any possibility of something being added later that affects logic
    private static IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>> EmptyMeterReadings =>
      new Dictionary<string, IList<(int DayCount, double Watts, double WattHour)>>();

    private static readonly CultureInfo CultureInfo = new CultureInfo(Constants.AggregationOptions.CultureName);
    private static Calendar Calendar => CultureInfo.Calendar;

    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    private CloudTable _powerTable;
    private CloudTable _powerMonthlyTable;
    private CloudTable _powerYearlyTable;

    public CloudTable PowerTable
    {
      get => _powerTable;
      set => _powerTable = ValidateTable(value, Constants.Table.Power);
    }

    public CloudTable PowerMonthlyTable
    {
      get => _powerMonthlyTable;
      set => _powerMonthlyTable = ValidateTable(value, Constants.Table.PowerMonthly);
    }

    public CloudTable PowerYearlyTable
    {
      get => _powerYearlyTable;
      set => _powerYearlyTable = ValidateTable(value, Constants.Table.PowerYearly);
    }

    public PowerAggregationProvider(ISolarViewRepositoryFactory repositoryFactory)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    // assumes to be processing full days
    // dates are assumed to be in the site local time
    public async Task<IEnumerable<TimeWatts>> GetAverageDayView(string siteId, MeterType meterType, DateTime startDate, DateTime endDate)
    {
      // determine what full years and months we have, then the remaining individual days
      var yearPeriods = GetYearPeriods(startDate, endDate).AsReadOnlyList();
      var monthPeriods = GetMonthPeriods(startDate, endDate, yearPeriods).AsReadOnlyList();
      var dayPeriods = GetDayPeriods(startDate, endDate, yearPeriods, monthPeriods);

      // load all of the required data in parallel
      var dailyReadingsTask = GetDailyReadings(siteId, meterType, dayPeriods);
      var monthlyReadingsTask = GetMonthlyReadings(siteId, meterType, monthPeriods);
      var yearlyReadingsTask = GetYearlyReadings(siteId, meterType, yearPeriods);

      var (daily, monthly, yearly) = await TaskHelper.WhenAll(dailyReadingsTask, monthlyReadingsTask, yearlyReadingsTask);
      
      return GetMeterReadings(daily, monthly, yearly);
    }

    private static IEnumerable<TimeWatts> GetMeterReadings(
      IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>> daily,
      IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>> monthly, 
      IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>> yearly)
    {
      var meterReadings = new List<TimeWatts>();

      for (var minutes = 0; minutes < 24 * 4 * 15; minutes += 15)
      {
        var timespan = TimeSpan.FromMinutes(minutes);
        var time = $"{timespan.Hours:D2}{timespan.Minutes:D2}";

        var wattValues = new List<(int DayCount, double Watts, double WattHour)>();

        if (daily.ContainsKey(time))
        {
          wattValues.AddRange(daily[time]);
        }

        if (monthly.ContainsKey(time))
        {
          wattValues.AddRange(monthly[time]);
        }

        if (yearly.ContainsKey(time))
        {
          wattValues.AddRange(yearly[time]);
        }

        var totalDays = 0;
        var totalWatts = 0.0d;
        var totalWattHour = 0.0d;

        foreach (var (dayCount, watts, wattHour) in wattValues)
        {
          totalDays += dayCount;
          totalWatts += watts;
          totalWattHour += wattHour;
        }

        var formattedTime = $"{timespan.Hours:D2}:{timespan.Minutes:D2}";
        var averageWatts = totalDays == 0 ? 0.0d : totalWatts / totalDays;
        var averageWattHour = totalDays == 0 ? 0.0d : totalWattHour / totalDays;

        var timeWatts = new TimeWatts(
          formattedTime,
          Math.Round(averageWatts, 6, MidpointRounding.AwayFromZero),
          Math.Round(averageWattHour, 6, MidpointRounding.AwayFromZero));

        meterReadings.Add(timeWatts);
      }

      return meterReadings;
    }

    private async Task<IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>>> GetDailyReadings(string siteId,
      MeterType meterType, IReadOnlyCollection<DateRange> dayPeriods)
    {
      if (dayPeriods.Count == 0)
      {
        return EmptyMeterReadings;
      }

      var meterReadings = new Dictionary<string, IList<(int DayCount, double Watts, double WattHour)>>();

      var dailyRepository = _repositoryFactory.Create<IPowerRepository>(PowerTable);

      foreach (var dayPeriod in dayPeriods)
      {
        for (var date = dayPeriod.StartDateTime; date <= dayPeriod.EndDateTime; date = date.AddDays(1))
        {
          await foreach (var powerItem in dailyRepository.GetMeterPowerAsyncEnumerable(siteId, date, meterType))
          {
            if (!meterReadings.ContainsKey(powerItem.Time))
            {
              meterReadings.Add(powerItem.Time, new List<(int, double, double)>());
            }

            meterReadings[powerItem.Time].Add((1, powerItem.Watts, powerItem.WattHour));
          }
        }
      }

      return meterReadings;
    }

    private async Task<IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>>> GetMonthlyReadings(string siteId,
      MeterType meterType, IReadOnlyCollection<AggregationMonth> monthPeriods)
    {
      if (monthPeriods.Count == 0)
      {
        return EmptyMeterReadings;
      }

      var meterReadings = new Dictionary<string, IList<(int DayCount, double Watts, double WattHour)>>();
      var monthlyRepository = _repositoryFactory.Create<IPowerMonthlyRepository>(PowerMonthlyTable);

      foreach (var monthPeriod in monthPeriods)
      {
        await foreach (var powerItem in monthlyRepository.GetMeterData(siteId, monthPeriod.Year, monthPeriod.Month, meterType))
        {
          if (!meterReadings.ContainsKey(powerItem.Time))
          {
            meterReadings.Add(powerItem.Time, new List<(int, double, double)>());
          }

          meterReadings[powerItem.Time].Add((powerItem.DayCount, powerItem.Watts, powerItem.WattHour));
        }
      }

      return meterReadings;
    }

    private async Task<IDictionary<string, IList<(int DayCount, double Watts, double WattHour)>>> GetYearlyReadings(string siteId,
      MeterType meterType, IReadOnlyCollection<AggregationYear> yearPeriods)
    {
      if (yearPeriods.Count == 0)
      {
        return EmptyMeterReadings;
      }

      var meterReadings = new Dictionary<string, IList<(int DayCount, double Watts, double WattHour)>>();
      var yearlyRepository = _repositoryFactory.Create<IPowerYearlyRepository>(PowerYearlyTable);

      foreach (var yearPeriod in yearPeriods)
      {
        await foreach (var powerItem in yearlyRepository.GetMeterData(siteId, yearPeriod.Year, meterType))
        {
          if (!meterReadings.ContainsKey(powerItem.Time))
          {
            meterReadings.Add(powerItem.Time, new List<(int, double, double)>());
          }

          meterReadings[powerItem.Time].Add((powerItem.DayCount, powerItem.Watts, powerItem.WattHour));
        }
      }

      return meterReadings;
    }

    private static IEnumerable<AggregationYear> GetYearPeriods(DateTime startDate, DateTime endDate)
    {
      var yearStartDate = startDate.Day == 1 && startDate.Month == 12
        ? startDate
        : new DateTime(startDate.Year + 1, 1, 1);

      var yearEndDate = new DateTime(yearStartDate.Year, 12, 31);

      while (yearStartDate < endDate && yearEndDate <= endDate)
      {
        yield return new AggregationYear(yearStartDate, yearEndDate, yearStartDate.Year);

        yearStartDate = yearStartDate.AddYears(1);
        yearEndDate = yearEndDate.AddYears(1);
      }
    }

    private static IEnumerable<AggregationMonth> GetMonthPeriods(DateTime startDate, DateTime endDate, IReadOnlyCollection<AggregationYear> yearPeriods)
    {
      var excludeDateRange = yearPeriods.Count == 0
        ? ExcludedNoDates
        : new DateRange(yearPeriods.First().StartDate, yearPeriods.Last().EndDate);

      var monthStartDate = startDate.Day == 1
        ? startDate
        : new DateTime(startDate.Year, startDate.Month + 1, 1);

      static DateTime GetLastDayOfMonth(DateTime date)
      {
        var daysInMonth = Calendar.GetDaysInMonth(date.Year, date.Month);
        return new DateTime(date.Year, date.Month, daysInMonth);
      }

      var monthEndDate = GetLastDayOfMonth(monthStartDate);

      while (monthStartDate < endDate && monthEndDate <= endDate)
      {
        if (!(monthStartDate >= excludeDateRange.StartDateTime && monthEndDate <= excludeDateRange.EndDateTime))
        {
          yield return new AggregationMonth(monthStartDate, monthEndDate, monthStartDate.Year, monthStartDate.Month);
        }

        monthStartDate = monthStartDate.AddMonths(1);
        monthEndDate = GetLastDayOfMonth(monthStartDate);
      }
    }

    private static IReadOnlyCollection<DateRange> GetDayPeriods(in DateTime startDate, in DateTime endDate,
      IReadOnlyCollection<AggregationYear> yearPeriods, IReadOnlyCollection<AggregationMonth> monthPeriods)
    {
      var minDateCovered = DateTime.MaxValue;
      var maxDateCovered = DateTime.MinValue;

      if (yearPeriods.Count > 0)
      {
        minDateCovered = yearPeriods.First().StartDate;
        maxDateCovered = yearPeriods.Last().EndDate;
      }

      if (monthPeriods.Count > 0)
      {
        minDateCovered = monthPeriods.First().StartDate;
        maxDateCovered = monthPeriods.Last().EndDate;
      }

      var dayPeriods = new List<DateRange>();

      if (minDateCovered == DateTime.MaxValue)
      {
        dayPeriods.Add(new DateRange(startDate, endDate));
      }
      else
      {
        dayPeriods.Add(new DateRange(startDate, minDateCovered.AddDays(-1)));
        dayPeriods.Add(new DateRange(maxDateCovered.AddDays(1), endDate));
      }

      return dayPeriods;
    }

    private static CloudTable ValidateTable(CloudTable value, string name)
    {
      if (value.Name != name)
      {
        throw new ArgumentException($"The provided table does not have the expected name '{name}'");
      }

      return value;
    }
  }
}