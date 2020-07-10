using SolarViewBlazor.Components.Charts;
using System;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts.Descriptors
{
  public class FeedInChartDescriptor : IChartDescriptor
  {
    public string Id => nameof(FeedInChartDescriptor);
    public string Description => "Feed In";
    public Type ChartType => typeof(FeedInChart);
    public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
  }
}