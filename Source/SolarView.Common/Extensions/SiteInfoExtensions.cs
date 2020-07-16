using System;
using SolarView.Common.Models;

namespace SolarView.Common.Extensions
{
  public static class SiteInfoExtensions
  {
    public static DateTime UtcToLocalTime(this ISiteDetails siteDetails, DateTime utcTime)
    {
      var tzi = TimeZoneInfo.FindSystemTimeZoneById(siteDetails.TimeZoneId);
      return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi);
    }
  }
}