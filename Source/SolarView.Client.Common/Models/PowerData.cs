namespace SolarView.Client.Common.Models
{
  public class PowerData
  {
    public string Time { get; set; }
    public double Consumption { get; set; }
    public double Production { get; set; }
    public double FeedIn { get; set; }
    public double Purchased { get; set; }
    public double SelfConsumption { get; set; }
  }
}
