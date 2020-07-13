using SolarView.Client.Common.Models;

namespace SolarViewBlazor.Charts.Models
{
  // used to calculate relative costs (Cost Benefit chart)
  public class PowerCost
  {
    public string Time { get; }
    public double WithSolarCost { get; }
    public double WithoutSolarCost { get; }
    public double Saving { get; }

    public PowerCost(PowerData powerData, PowerCostConfiguration configuration)
    {
      Time = powerData.Time;
      WithSolarCost = configuration.FixedCostPerQuarterHour + (powerData.WattHour.Purchased * configuration.PurchaseCostPerW - powerData.WattHour.FeedIn * configuration.FeedInCostPerW);
      WithoutSolarCost = configuration.FixedCostPerQuarterHour + ((powerData.WattHour.Purchased + powerData.WattHour.SelfConsumption) * configuration.PurchaseCostPerW);
      Saving = WithoutSolarCost - WithSolarCost;
    }
  }
}