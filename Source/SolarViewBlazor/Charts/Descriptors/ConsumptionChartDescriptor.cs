using System;
using System.Collections.Generic;
using SolarViewBlazor.Components.Charts;

namespace SolarViewBlazor.Charts.Descriptors
{
  public class ConsumptionChartDescriptor : IChartDescriptor
  {
    public string Id => nameof(ConsumptionChartDescriptor);
    public string Description => "Consumption";
    public Type ChartType => typeof(ConsumptionChart);
    public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
  }
}