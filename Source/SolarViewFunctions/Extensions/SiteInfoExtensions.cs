using AllOverIt.Extensions;
using SolarView.Common.Extensions;
using SolarView.Common.Models;
using System;

namespace SolarViewFunctions.Extensions
{
  public static class SiteInfoExtensions
  {
    public static DateTime GetLastAggregationDate(this ISiteInfo siteInfo)
    {
      // returns in site's local date
      return siteInfo.LastAggregationDate.IsNullOrEmpty()
        ? siteInfo.StartDate.ParseSolarDate().Date
        : siteInfo.LastAggregationDate.ParseSolarDate();
    }

    public static DateTime GetLastRefreshDateTime(this ISiteInfo siteInfo)
    {
      // returns in site's local date
      return siteInfo.LastRefreshDateTime.IsNullOrEmpty()
        ? siteInfo.StartDate.ParseSolarDate().Date
        : siteInfo.LastRefreshDateTime.ParseSolarDateTime().TrimToHour();
    }

    public static DateTime GetLastSummaryDate(this ISiteInfo siteInfo)
    {
      // returns in site's local date
      return siteInfo.LastSummaryDate.IsNullOrEmpty() 
        ? siteInfo.StartDate.ParseSolarDate()
        : siteInfo.LastSummaryDate.ParseSolarDate();
    }

    public static DateTime UtcToLocalTime(this ISiteInfo siteInfo, DateTime utcTime)
    {
      var tzi = TimeZoneInfo.FindSystemTimeZoneById(siteInfo.TimeZoneId);
      return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
    }
  }
}