using SolarView.Client.Common.Models;
using System;
using System.Collections.Generic;

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
  }
}