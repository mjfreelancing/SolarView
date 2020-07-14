using System;

namespace SolarView.Common.Models
{
  public class DateRange
  {
    public DateTime StartDateTime { get; }
    public DateTime EndDateTime { get; }

    public DateRange(DateTime startDateTime, DateTime endDateTime)
    {
      StartDateTime = startDateTime;
      EndDateTime = endDateTime;
    }
  }
}