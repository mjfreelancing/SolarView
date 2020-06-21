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

    private double PurchaseCostPerW = 0.32d / 1000.0d;
    private double FeedInCostPerW = 0.10d / 1000.0d;
    public double AdjustedCost => Purchased * PurchaseCostPerW - FeedIn * FeedInCostPerW;
    public double NoSolarCost => (Purchased + SelfConsumption) * PurchaseCostPerW;
  }
}
