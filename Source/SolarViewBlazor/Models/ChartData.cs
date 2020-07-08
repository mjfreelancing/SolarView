using SolarView.Client.Common.Models;
using System;
using System.Collections.Generic;

namespace SolarViewBlazor.Models
{
  public class ChartData
  {
    public string Id { get; set; }              // uniquely identify each cached chart
    public string DescriptorId { get; set; }    // identify the descriptor used to visualize this data
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IReadOnlyList<PowerData> Power { get; set; }
  }
}