using SolarViewFunctions.Models;
using System;
using System.Collections.Generic;

namespace SolarViewFunctions.Helpers
{
  public static class SolarViewHelpers
  {
    public static IEnumerable<DateRange> GetMonthlyDateRanges(DateTime startDateTime, DateTime endDateTime)
    {
      var startRequestDate = startDateTime;

      do
      {
        var endRequestDate = GetEndOfMonth(startRequestDate);

        if (endRequestDate > endDateTime)
        {
          endRequestDate = endDateTime;
        }

        yield return new DateRange(startRequestDate, endRequestDate);

        startRequestDate = endRequestDate.AddDays(1).Date;

      } while (startRequestDate < endDateTime);
    }

    private static DateTime GetEndOfMonth(DateTime dateTime)
    {
      return new DateTime(
        dateTime.Year, dateTime.Month,
        DateTime.DaysInMonth(dateTime.Year, dateTime.Month), 23, 59, 59);
    }
  }
}