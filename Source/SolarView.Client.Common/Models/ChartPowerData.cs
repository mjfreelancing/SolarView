using System;
using System.Collections.Generic;

namespace SolarView.Client.Common.Models
{
  // power data that can be shared across multiple chart descriptors
  public class ChartPowerData
  {
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IReadOnlyList<PowerData> Power { get; set; }

    public ChartPowerData()
    {
    }

    public ChartPowerData(DateTime startDate, DateTime endDate, IReadOnlyList<PowerData> power)
    {
      StartDate = startDate;
      EndDate = endDate;
      Power = power;
    }
  }
}