using SolarViewBlazor.Components.Charts;
using System;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts.Descriptors
{
  public class AverageWattsChartDescriptor : IChartDescriptor
  {
    public string Id => nameof(AverageWattsChartDescriptor);
    public string Description => "Average Watts";
    public Type ChartType => typeof(AverageWattsChart);
    public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
  }
}