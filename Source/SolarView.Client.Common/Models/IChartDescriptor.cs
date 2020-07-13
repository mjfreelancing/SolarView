using System;
using System.Collections.Generic;

namespace SolarView.Client.Common.Models
{
  public interface IChartDescriptor
  {
    string Id { get; }
    string Description { get; }
    Type ChartType { get; }
    IDictionary<string, object> Parameters { get; }
  }
}