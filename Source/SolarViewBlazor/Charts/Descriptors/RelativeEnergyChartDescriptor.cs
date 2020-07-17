using SolarView.Client.Common.Models;
using System;
using System.Collections.Generic;
using SolarViewBlazor.Components.Charts;

namespace SolarViewBlazor.Charts.Descriptors
{
  public class RelativeEnergyChartDescriptor : IChartDescriptor
  {
    public string Id => nameof(RelativeEnergyChartDescriptor);
    public string Description => "Relative Energy";
    public Type ChartType => typeof(RelativeEnergyChart);
    public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
  }
}