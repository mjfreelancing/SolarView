using AllOverIt.Extensions;
using SolarViewFunctions.Entities;
using System;

namespace SolarViewFunctions.Extensions
{
  public static class SiteInfoExtensions
  {
    public static (DateTime StartTime, DateTime EndTime) GetNextRefreshPeriod(this SiteInfo siteInfo, DateTime siteLocalTime)
    {
      var startDateTime = siteInfo.LastRefreshDateTime.IsNullOrEmpty()
        ? siteInfo.StartDate.ParseSolarDate()
        : siteInfo.LastRefreshDateTime.ParseSolarDateTime();

      // floor the current time to the hour 
      var endDateTime = siteLocalTime.TrimToHour();

      return (startDateTime, endDateTime);
    }

    public static DateTime GetNextRefreshDueUtc(this SiteInfo siteInfo)
    {
      if (siteInfo.LastRefreshDateTime.IsNullOrEmpty())
      {
        return DateTime.MinValue;
      }

      // truncate the next refresh to the hour
      var lastRefreshTime = siteInfo.LastRefreshDateTime.ParseSolarDateTime();
      var nextRefreshTime = lastRefreshTime.TrimToHour().AddHours(1);

      var tzi = TimeZoneInfo.FindSystemTimeZoneById(siteInfo.TimeZoneId);

      return TimeZoneInfo.ConvertTimeToUtc(nextRefreshTime, tzi);
    }

    // only sends back the date portion - time is irrelevant because the full day is processed
    public static (DateTime StartDate, DateTime EndDate) GetNextAggregationPeriod(this SiteInfo siteInfo, DateTime siteLocalTime)
    {
      var startDate = siteInfo.LastAggregationDate.IsNullOrEmpty()
        ? siteInfo.StartDate.ParseSolarDate().Date
        : siteInfo.LastAggregationDate.ParseSolarDate();

      // aggregating until the end of the previous day
      var endDate = siteLocalTime.Date.AddDays(-1);

      return (startDate.Date, endDate);
    }

    public static DateTime UtcToLocalTime(this SiteInfo siteInfo, DateTime utcTime)
    {
      var tzi = TimeZoneInfo.FindSystemTimeZoneById(siteInfo.TimeZoneId);
      return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
    }
  }
}