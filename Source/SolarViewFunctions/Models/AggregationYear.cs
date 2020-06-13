using System;

namespace SolarViewFunctions.Models
{
  public class AggregationYear : AggregationPeriodBase
  {
    public AggregationYear(DateTime startDate, DateTime endDate, int year)
      : base(startDate, endDate, year)
    {
    }
  }
}