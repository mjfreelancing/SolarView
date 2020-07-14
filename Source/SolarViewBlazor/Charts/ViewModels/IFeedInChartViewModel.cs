using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts.ViewModels
{
  public interface IFeedInChartViewModel
  {
    IReadOnlyList<TimeFeedIn> CalculateData(IEnumerable<PowerData> powerData, PowerUnit powerUnit, bool isCumulative);
  }
}