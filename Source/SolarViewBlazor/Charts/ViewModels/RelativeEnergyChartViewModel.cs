using AllOverIt.Extensions;
using SolarView.Client.Common.Helpers;
using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System.Collections.Generic;
using System.Linq;

namespace SolarViewBlazor.Charts.ViewModels
{
  public class RelativeEnergyChartViewModel : IRelativeEnergyChartViewModel
  {
    public IReadOnlyList<RelativeEnergy> CalculateData(IEnumerable<PowerData> powerData, bool isCumulative)
    {
      return isCumulative
        ? CalculateCumulativeData(powerData)
        : CalculateNonCumulativeData(powerData);
    }

    private static IReadOnlyList<RelativeEnergy> CalculateCumulativeData(IEnumerable<PowerData> powerData)
    {
      var lastPowerItem = new PowerData { WattHour = new WattsData() };

      return powerData
        .Select(item =>
        {
          lastPowerItem.Time = item.Time;
          lastPowerItem.WattHour = WattsDataHelpers.Aggregate(lastPowerItem.WattHour, item.WattHour);

          return new RelativeEnergy(lastPowerItem);
        })
        .Where(IncludeData)
        .AsReadOnlyList();
    }

    private static IReadOnlyList<RelativeEnergy> CalculateNonCumulativeData(IEnumerable<PowerData> powerData)
    {
      return powerData
        .Select(item => new RelativeEnergy(item))
        .Where(IncludeData)
        .AsReadOnlyList();
    }

    private static bool IncludeData(RelativeEnergy item)
    {
      // excluding the point if everything is zero
      return !item.SelfConsumptionVsProduction.IsZero() ||
             !item.ProductionVsConsumption.IsZero() ||
             !item.SelfConsumptionVsConsumption.IsZero();
    }
  }
}