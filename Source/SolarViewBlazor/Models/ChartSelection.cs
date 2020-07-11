using SolarViewBlazor.Charts;

namespace SolarViewBlazor.Models
{
  // used on the 'Compare' page to provide info about the types of charts available
  public class ChartSelection
  {
    public bool Selected { get; set; }
    public IChartDescriptor ChartDescriptor { get; set; }
  }
}