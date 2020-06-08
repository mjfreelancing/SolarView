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
  }
}