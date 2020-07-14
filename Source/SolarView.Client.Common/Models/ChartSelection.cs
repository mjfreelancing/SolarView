namespace SolarView.Client.Common.Models
{
  public class ChartSelection
  {
    // indicates if this chart type has been selected for creation
    public bool Selected { get; set; }

    // describes the chart to be created
    public IChartDescriptor ChartDescriptor { get; set; }
  }
}