using AllOverIt.Helpers;
using Microsoft.AspNetCore.Components;

namespace SolarViewBlazor.Charts
{
  public class ChartReference : IChartReference
  {
    public RenderFragment Instance { get; }

    public ChartReference(RenderFragment instance)
    {
      Instance = instance.WhenNotNull(nameof(instance));
    }
  }
}