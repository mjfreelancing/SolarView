using SolarView.Client.Common.Models;
using SolarViewBlazor.Charts.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.Charts.ViewModels
{
  public interface ICostBenefitChartViewModel
  {
    Task<IReadOnlyList<PowerCost>> CalculateData(IEnumerable<PowerData> powerData, DateTime startDate, DateTime endDate, bool isCumulative);
  }
}