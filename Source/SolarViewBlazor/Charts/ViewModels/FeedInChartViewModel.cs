﻿using AllOverIt.Extensions;
using SolarView.Client.Common.Helpers;
using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System.Collections.Generic;
using System.Linq;

namespace SolarViewBlazor.Charts.ViewModels
{
  public class FeedInChartViewModel : IFeedInChartViewModel
  {
    public IReadOnlyList<TimeFeedIn> CalculateData(IEnumerable<PowerData> powerData, PowerUnit powerUnit, bool isCumulative)
    {
      var feedInData = isCumulative
        ? CalculateCumulativeData(powerData, powerUnit)
        : CalculateNonCumulativeData(powerData, powerUnit);

      return ChartHelpers.TrimDataEnds(feedInData, item => item.FeedIn.IsZero(), true, true);
    }

    private static IReadOnlyList<TimeFeedIn> CalculateCumulativeData(IEnumerable<PowerData> powerData, PowerUnit powerUnit)
    {
      var lastPowerItem = new PowerData { Watts = new WattsData() };

      return powerData
        .Select(item =>
        {
          var scaledData = powerUnit == PowerUnit.Watts
            ? WattsDataHelpers.Scale(item.Watts, 0.001d)
            : WattsDataHelpers.Scale(item.WattHour, 0.001d);

          lastPowerItem.Watts = WattsDataHelpers.Aggregate(lastPowerItem.Watts, scaledData);

          return new TimeFeedIn(item.Time, lastPowerItem.Watts.FeedIn);
        })
        .AsReadOnlyList();
    }

    private static IReadOnlyList<TimeFeedIn> CalculateNonCumulativeData(IEnumerable<PowerData> powerData, PowerUnit powerUnit)
    {
      return powerData
        .Select(item =>
        {
          var wattData = powerUnit == PowerUnit.Watts
            ? item.Watts
            : item.WattHour;

          return new TimeFeedIn(item.Time, wattData.FeedIn);
        })
        .AsReadOnlyList();
    }
  }
}