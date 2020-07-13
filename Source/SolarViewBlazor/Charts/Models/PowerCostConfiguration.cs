namespace SolarViewBlazor.Charts.Models
{
  // factors used to calculate relative costs (Cost Benefit chart)
  public class PowerCostConfiguration
  {
    public double PurchaseCostPerW { get; set; }
    public double FeedInCostPerW { get; set; }
    public double FixedCostPerQuarterHour { get; set; }
  }
}