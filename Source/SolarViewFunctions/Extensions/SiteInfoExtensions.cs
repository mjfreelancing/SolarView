using AllOverIt.Extensions;
using SolarViewFunctions.Entities;
using System;

namespace SolarViewFunctions.Extensions
{
  public static class SiteInfoExtensions
  {
    public static (DateTime startTime, DateTime endTime) GetNextRefreshPeriod(this SiteInfo siteInfo, DateTime siteLocalTime)
    {
      var startDateTime = siteInfo.LastRefreshEnd.IsNullOrEmpty()
        ? siteInfo.InstallDate.ParseSolarDateTime()
        : siteInfo.LastRefreshEnd.ParseSolarDateTime();

      // floor the current time to the minute 
      var endDateTime = siteLocalTime.AddSeconds(-siteLocalTime.Second);

      return (startDateTime, endDateTime);
    }

    public static DateTime NextRefreshDueUtc(this SiteInfo siteInfo)
    {
      if (siteInfo.NextRefreshDue.IsNullOrEmpty())
      {
        return DateTime.MinValue;
      }

      var nextRefreshTime = siteInfo.NextRefreshDue.ParseSolarDateTime();
      var tzi = TimeZoneInfo.FindSystemTimeZoneById(siteInfo.TimeZoneId);

      return TimeZoneInfo.ConvertTimeToUtc(nextRefreshTime, tzi);
    }

    public static void UpdateRefreshTimes(this SiteInfo siteInfo, string lastRefreshLocalTime)
    {
      //siteInfo.LastRefreshEnd = FormatTimeString(lastRefreshLocalTime);
      siteInfo.LastRefreshEnd = lastRefreshLocalTime;

      var dateTime = lastRefreshLocalTime.ParseSolarDateTime();

      // restrict to hourly boundaries
      var nextDateTime = new DateTime(
          dateTime.Year, dateTime.Month, dateTime.Day,
          dateTime.Hour, 0, 0)
        .AddHours(1);

      siteInfo.NextRefreshDue = nextDateTime.GetSolarDateTimeString();
    }

    public static DateTime UtcToLocalTime(this SiteInfo siteInfo, DateTime utcTime)
    {
      var tzi = TimeZoneInfo.FindSystemTimeZoneById(siteInfo.TimeZoneId);
      return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
    }
  }
}