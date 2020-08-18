using SolarView.Client.Common.Models;
using SolarView.Common.Models;
using System;

namespace SolarViewBlazor.Charts.Models
{
  // used to calculate relative costs (Cost Benefit chart)
  public class PowerCost
  {
    // hard-coded for now
    private const string PeakStartTime = "07:00";
    private const string PeakEndTime = "22:00";
    private bool UseOffPeakRate = false;

    public string Time { get; }
    public double WithSolarCost { get; private set; }
    public double WithoutSolarCost { get; private set; }
    public double Saving { get; private set; }

    public PowerCost()
    {
    }

    public PowerCost(PowerData powerData, ISiteEnergyCosts siteEnergyCosts)
    {
      Time = powerData.Time;

      var supplyChargePerQuarterHour = siteEnergyCosts.SupplyCharge / 24.0d / 4.0d;

      // off-peak is such a small component and difficult to quantify - hence it is optional
      // (could always fudge the rate by applying, for example, 90% peak rate + 10% off-peak rate)
      var usePeakRate = !UseOffPeakRate ||
                        string.Compare(Time, PeakStartTime, StringComparison.Ordinal) >= 0 &&
                        string.Compare(Time, PeakEndTime, StringComparison.Ordinal) <= 0;

      var ratePerWh = (usePeakRate ? siteEnergyCosts.PeakRate : siteEnergyCosts.OffPeakRate) / 1000.0d;

      var buyBackPerWh = siteEnergyCosts.SolarBuyBackRate / 1000.0d;

      WithSolarCost = supplyChargePerQuarterHour + (powerData.WattHour.Purchased * ratePerWh - powerData.WattHour.FeedIn * buyBackPerWh);
      WithoutSolarCost = supplyChargePerQuarterHour + (powerData.WattHour.Purchased + powerData.WattHour.SelfConsumption) * ratePerWh;
      Saving = WithoutSolarCost - WithSolarCost;
    }

    public void AddCost(PowerCost powerCost)
    {
      WithSolarCost += powerCost.WithSolarCost;
      WithoutSolarCost += powerCost.WithoutSolarCost;
      Saving += powerCost.Saving;
    }
  }
}