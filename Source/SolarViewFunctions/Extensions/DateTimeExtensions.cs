using System;

namespace SolarViewFunctions.Extensions
{
  public static class DateTimeExtensions
  {
    public static string GetSolarDateString(this DateTime timestamp)
    {
      return $"{timestamp:yyyy-MM-dd}";
    }

    public static string GetSolarDateTimeString(this DateTime timestamp)
    {
      return $"{timestamp:yyyy-MM-dd HH:mm:ss}";
    }

    public static DateTime TrimToHour(this DateTime dateTime)
    {
      return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
    }

    public static DateTime TrimToDayOfMonth(this DateTime dateTime, int day)
    {
      return new DateTime(dateTime.Year, dateTime.Month, day, 0, 0, 0);
    }
  }
}