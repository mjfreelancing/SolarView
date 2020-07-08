using System;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts
{
  public interface IChartDescriptor
  {
    string Id { get; }
    string Description { get; }
    Type ChartType { get; }
    IDictionary<string, object> Parameters { get; }
  }
}