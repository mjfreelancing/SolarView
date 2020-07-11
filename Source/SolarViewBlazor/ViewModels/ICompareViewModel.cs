using SolarView.Common.Models;
using SolarViewBlazor.Charts;
using SolarViewBlazor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public interface ICompareViewModel
  {
    Task LoadFromCacheAsync();
    Task<bool> AddChartsAsync(DateRange dateRange, IEnumerable<IChartDescriptor> chartDescriptors);
    Task DeleteChartAsync(string chartId);
    IReadOnlyList<IChartDescriptor> GetDescriptors();
    IReadOnlyList<ChartData> GetDescriptorData(IChartDescriptor descriptor);
  }
}