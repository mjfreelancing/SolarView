using System;

namespace SolarViewFunctions.Models
{
  public abstract class AggregationPeriodBase
  {
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public int Year { get; }

    protected AggregationPeriodBase(DateTime startDate, DateTime endDate, int year)
    {
      StartDate = startDate;
      EndDate = endDate;
      Year = year;
    }
  }
}