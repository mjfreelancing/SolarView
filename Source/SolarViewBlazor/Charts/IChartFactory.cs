using Microsoft.AspNetCore.Components;
using SolarView.Client.Common.Models;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts
{
  public interface IChartFactory
  {
    IReadOnlyList<IChartDescriptor> ChartDescriptors { get; }

    RenderFragment CreateChart(IChartDescriptor descriptor, ChartData chartData);
  }
}