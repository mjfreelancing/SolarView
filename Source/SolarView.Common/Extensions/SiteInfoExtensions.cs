using System;
using SolarView.Common.Models;

namespace SolarView.Common.Extensions
{
  public static class SiteInfoExtensions
  {
    public static DateTime UtcToLocalTime(this ISiteInfo siteInfo, DateTime utcTime)
    {
      var tzi = TimeZoneInfo.FindSystemTimeZoneById(siteInfo.TimeZoneId);
      return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
    }
  }
}