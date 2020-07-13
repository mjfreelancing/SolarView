using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts.ViewModels
{
  public interface IConsumptionChartViewModel
  {
    IReadOnlyList<TimeWatts> CalculateData(IEnumerable<PowerData> powerData, PowerUnit powerUnit, bool isCumulative);
  }
}