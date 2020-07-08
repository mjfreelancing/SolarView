using Microsoft.AspNetCore.Components;
using SolarViewBlazor.Models;

namespace SolarViewBlazor.Charts
{
  public interface IChartFactory
  {
    //IChartDescriptor RegisterChartType(Type chartType);
    RenderFragment CreateChart(IChartDescriptor descriptor, ChartData chartData);
  }
}