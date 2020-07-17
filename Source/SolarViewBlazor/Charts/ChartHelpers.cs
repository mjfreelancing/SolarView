using AllOverIt.Extensions;
using SolarView.Client.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarViewBlazor.Charts
{
  public static class ChartHelpers
  {
    public static IReadOnlyList<DisplayPowerUnit> GetDisplayPowerUnits() => new List<DisplayPowerUnit>
    {
      new DisplayPowerUnit(PowerUnit.Watts, "Watts"),
      new DisplayPowerUnit(PowerUnit.WattHour, "Watt-Hour"),
    };

    public static string GetYAxisLabel(PowerUnit powerUnit, bool isCumulative)
    {
      return (powerUnit, isCumulative) switch
      {
        (PowerUnit.Watts, true) => "{value}KW",
        (PowerUnit.Watts, false) => "{value}W",
        (PowerUnit.WattHour, true) => "{value}KWh",
        (PowerUnit.WattHour, false) => "{value}Wh",
        (_, _) => throw new InvalidOperationException($"Unknown Unit/Mode combination: {powerUnit}/{isCumulative}")
      };
    }

    public static IReadOnlyList<TType> TrimDataEnds<TType>(IReadOnlyList<TType> data, Func<TType, bool> excludePredicate,
      bool keepFirstExcluded, bool keepLastExcluded)
    {
      var startExcludeCount = data.TakeWhile(excludePredicate).Count();

      if (startExcludeCount == data.Count)
      {
        return new List<TType>();
      }

      var endExcludeCount = data.Reverse().TakeWhile(excludePredicate).Count();
      var nonExcludeCount = data.Count - startExcludeCount - endExcludeCount;

      // determine whether to keep the first and last excluded items

      var takeCount = nonExcludeCount;

      if (keepFirstExcluded && startExcludeCount > 0)
      {
        startExcludeCount--;
        takeCount++;
      }

      if (keepLastExcluded && endExcludeCount > 0)
      {
        takeCount++;
      }

      return data.Skip(startExcludeCount).Take(takeCount).AsReadOnlyList();
    }
  }
}