using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts.ViewModels
{
  public interface ICostBenefitChartViewModel
  {
    void SetCostConfiguration(PowerCostConfiguration costConfiguration);
    IReadOnlyList<PowerCost> CalculateData(IEnumerable<PowerData> powerData, bool isCumulative);
  }
}