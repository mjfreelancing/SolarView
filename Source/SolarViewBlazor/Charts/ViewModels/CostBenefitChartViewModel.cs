using AllOverIt.Extensions;
using AllOverIt.Helpers;
using SolarView.Client.Common.Helpers;
using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System.Collections.Generic;
using System.Linq;

namespace SolarViewBlazor.Charts.ViewModels
{
  public class CostBenefitChartViewModel : ICostBenefitChartViewModel
  {
    private PowerCostConfiguration _costConfiguration;

    public void SetCostConfiguration(PowerCostConfiguration costConfiguration)
    {
      _costConfiguration = costConfiguration.WhenNotNull(nameof(costConfiguration));
    }

    public IReadOnlyList<PowerCost> CalculateData(IEnumerable<PowerData> powerData, bool isCumulative)
    {
      return isCumulative
        ? CalculateCumulativeData(powerData)
        : CalculateNonCumulativeData(powerData);
    }

    private IReadOnlyList<PowerCost> CalculateCumulativeData(IEnumerable<PowerData> powerData)
    {
      var lastPowerItem = new PowerData { WattHour = new WattsData() };

      return powerData
        .Select(item =>
        {
          lastPowerItem.Time = item.Time;
          lastPowerItem.WattHour = WattsDataHelpers.Aggregate(lastPowerItem.WattHour, item.WattHour);

          return new PowerCost(lastPowerItem, _costConfiguration);
        })
        .AsReadOnlyList();
    }

    private IReadOnlyList<PowerCost> CalculateNonCumulativeData(IEnumerable<PowerData> powerData)
    {
     return powerData
       .Select(item => new PowerCost(item, _costConfiguration))
       .AsReadOnlyList();
    }
  }
}