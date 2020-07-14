using System;

namespace SolarView.Common.Extensions
{
  public static class DateTimeExtensions
  {
    public static DateTime GetEndOfMonth(this DateTime dateTime)
    {
      return new DateTime(
        dateTime.Year, dateTime.Month,
        DateTime.DaysInMonth(dateTime.Year, dateTime.Month), 23, 59, 59);
    }

    public static bool IsSameMonthYear(this DateTime dateTime, DateTime other)
    {
      return dateTime.Year == other.Year && dateTime.Month == other.Month;
    }
  }
}