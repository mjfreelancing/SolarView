using System;

namespace SolarViewFunctions.Extensions
{
  public static class StringExtensions
  {
    public static DateTime ParseSolarDateTime(this string timestamp)
    {
      return DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", null);
    }
  }
}