using SolarView.Client.Common.Models;
using System;
using System.Collections.Generic;

namespace SolarViewBlazor.Models
{
  // power data that can be shared across multiple chart descriptors
  public class ChartPowerData
  {
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public IReadOnlyList<PowerData> Power { get; set; }

    public ChartPowerData(DateTime startDate, DateTime endDate, IReadOnlyList<PowerData> power)
    {
      StartDate = startDate;
      EndDate = endDate;
      Power = power;
    }
  }
}