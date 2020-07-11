using Microsoft.AspNetCore.Components;
using SolarViewBlazor.Models;

namespace SolarViewBlazor.Charts
{
  public interface IChartFactory
  {
    RenderFragment CreateChart(IChartDescriptor descriptor, ChartData chartData);
  }
}