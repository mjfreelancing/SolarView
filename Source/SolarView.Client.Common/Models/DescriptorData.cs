namespace SolarView.Client.Common.Models
{
  public class DescriptorData
  {
    public string ChartDataId { get; set; }           // identifies the data used by the chart
    public string DescriptorId { get; set; }          // identifies the descriptor used to build the chart

    public DescriptorData()
    {
    }

    public DescriptorData(string chartDataId, string descriptorId)
    {
      ChartDataId = chartDataId;
      DescriptorId = descriptorId;
    }
  }
}