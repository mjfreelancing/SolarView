using SolarView.Client.Common.Models;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts
{
  public interface IChartRegistry
  {
    IEnumerable<IChartDescriptor> ChartDescriptors { get; }

    void RegisterDescriptor(IChartDescriptor descriptor);
  }
}