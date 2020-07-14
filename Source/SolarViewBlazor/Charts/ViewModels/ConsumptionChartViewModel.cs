using AllOverIt.Extensions;
using SolarView.Client.Common.Helpers;
using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System.Collections.Generic;
using System.Linq;

namespace SolarViewBlazor.Charts.ViewModels
{
  public class ConsumptionChartViewModel : IConsumptionChartViewModel
  {
    public IReadOnlyList<TimeWatts> CalculateData(IEnumerable<PowerData> powerData, PowerUnit powerUnit, bool isCumulative)
    {
      return isCumulative
        ? CalculateCumulativeData(powerData, powerUnit)
        : CalculateNonCumulativeData(powerData, powerUnit);
    }

    private IReadOnlyList<TimeWatts> CalculateCumulativeData(IEnumerable<PowerData> powerData, PowerUnit powerUnit)
    {
      var lastWattsItem = new WattsData();

      return powerData
        .Select(item =>
        {
          // displaying in KW / KWh when aggregating
          var scaledData = powerUnit == PowerUnit.Watts
            ? WattsDataHelpers.Scale(item.Watts, 0.001d)
            : WattsDataHelpers.Scale(item.WattHour, 0.001d);

          lastWattsItem = WattsDataHelpers.Aggregate(lastWattsItem, scaledData);

          return new TimeWatts(item.Time, lastWattsItem);
        })
        .AsReadOnlyList();
    }

    private IReadOnlyList<TimeWatts> CalculateNonCumulativeData(IEnumerable<PowerData> powerData, PowerUnit powerUnit)
    {
      return powerData
        .Select(item =>
        {
          var wattData = powerUnit == PowerUnit.Watts
            ? item.Watts
            : item.WattHour;

          return new TimeWatts(item.Time, wattData);
        })
        .AsReadOnlyList();
    }
  }
}