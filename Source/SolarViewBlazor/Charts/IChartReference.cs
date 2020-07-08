using Microsoft.AspNetCore.Components;

namespace SolarViewBlazor.Charts
{
  public interface IChartReference
  {
    RenderFragment Instance { get; }
  }
}