namespace SolarView.Client.Common.Models
{
  public class WattsData
  {
    public double Consumption { get; set; }
    public double Production { get; set; }
    public double FeedIn { get; set; }
    public double Purchased { get; set; }
    public double SelfConsumption { get; set; }
  }

  public static class WattsDataHelpers
  {
    public static WattsData Aggregate(WattsData lhs, WattsData rhs)
    {
      return new WattsData
      {
        Consumption = lhs.Consumption + rhs.Consumption,
        Production = lhs.Production + rhs.Production,
        FeedIn = lhs.FeedIn + rhs.FeedIn,
        Purchased = lhs.Purchased + rhs.Purchased,
        SelfConsumption = lhs.SelfConsumption + rhs.SelfConsumption
      };
    }
  }
}