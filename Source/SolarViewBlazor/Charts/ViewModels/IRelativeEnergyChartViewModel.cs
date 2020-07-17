using System.Collections.Generic;
using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;

namespace SolarViewBlazor.Charts.ViewModels
{
  public interface IRelativeEnergyChartViewModel
  {
    IReadOnlyList<RelativeEnergy> CalculateData(IEnumerable<PowerData> powerData, bool isCumulative);
  }
}