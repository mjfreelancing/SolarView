using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SolarViewBlazor.Models;

namespace SolarViewBlazor.Charts
{
  public class ChartFactory : IChartFactory
  {
    public RenderFragment CreateChart(IChartDescriptor descriptor, ChartData chartData)
    {
      void CreateFragment(RenderTreeBuilder builder)
      {
        var i = 0;

        builder.OpenComponent(i++, descriptor.ChartType);

        // expected parameters
        //builder.AddAttribute(i++, nameof(IChartDescriptor.Class), descriptor.Class);
        builder.AddAttribute(i++, nameof(ChartData), chartData);

        foreach (var (key, value) in descriptor.Parameters)
        {
          builder.AddAttribute(i++, key, value);
        }

        builder.CloseComponent();
      }

      return CreateFragment;
    }
  }
}