using SolarView.Client.Common.Models;
using SolarView.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public interface ICompareViewModel
  {
    ISiteDetails CurrentSite { get; }

    Task LoadFromCacheAsync();
    Task<bool> AddChartsAsync(DateRange dateRange, IEnumerable<IChartDescriptor> chartDescriptors);
    Task DeleteChartAsync(string chartId);
    IReadOnlyList<IChartDescriptor> GetDescriptors();
    IReadOnlyList<ChartData> GetDescriptorData(IChartDescriptor descriptor);
  }
}