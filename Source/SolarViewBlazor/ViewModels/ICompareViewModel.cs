using SolarView.Common.Models;
using SolarViewBlazor.Charts;
using SolarViewBlazor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public interface ICompareViewModel
  {
    Task<bool> AddCharts(DateRange dateRange, IEnumerable<IChartDescriptor> chartDescriptors);
    Task DeleteChart(string chartId);
    IEnumerable<IChartDescriptor> GetDescriptors();
    IEnumerable<ChartData> GetChartData(IChartDescriptor descriptor);
  }
}