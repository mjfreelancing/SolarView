using AllOverIt.Extensions;
using SolarView.Client.Common.Models;

namespace SolarViewBlazor.Charts.Models
{
  public class RelativeEnergy
  {
    public string Time { get; }
    public double ProductionVsConsumption { get; }
    public double SelfConsumptionVsProduction { get; }
    public double SelfConsumptionVsConsumption { get; }

    public RelativeEnergy(PowerData powerData)
    {
      Time = powerData.Time;

      ProductionVsConsumption = powerData.WattHour.Consumption.IsZero()
        ? 0.0d
        : powerData.WattHour.Production * 100.0d / powerData.WattHour.Consumption;

      SelfConsumptionVsProduction = powerData.WattHour.Production.IsZero()
        ? 0.0d
        : powerData.WattHour.SelfConsumption * 100.0d / powerData.WattHour.Production;

      SelfConsumptionVsConsumption = powerData.WattHour.Consumption.IsZero()
        ? 0.0d
        : powerData.WattHour.SelfConsumption * 100.0d / powerData.WattHour.Consumption;
    }
  }
}