using SolarView.Client.Common.Models;

namespace SolarView.Client.Common.Helpers
{
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

    public static WattsData Scale(WattsData wattsData, double factor)
    {
      return new WattsData
      {
        Consumption = wattsData.Consumption * factor,
        Production = wattsData.Production * factor,
        FeedIn = wattsData.FeedIn * factor,
        Purchased = wattsData.Purchased * factor,
        SelfConsumption = wattsData.SelfConsumption * factor
      };
    }
  }
}