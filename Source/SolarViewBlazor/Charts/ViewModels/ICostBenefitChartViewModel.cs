using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.Charts.ViewModels
{
  public interface ICostBenefitChartViewModel
  {
    Task<IReadOnlyList<PowerCost>> CalculateData(IEnumerable<PowerData> powerData, bool isCumulative);
  }
}