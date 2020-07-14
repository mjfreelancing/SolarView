using System;

namespace SolarView.Common.Extensions
{
  public static class StringExtensions
  {
    public static DateTime ParseSolarDate(this string timestamp)
    {
      return DateTime.ParseExact(timestamp, "yyyy-MM-dd", null);
    }

    public static DateTime ParseSolarDateTime(this string timestamp)
    {
      return DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", null);
    }
  }
}