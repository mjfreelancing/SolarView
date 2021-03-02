using SolarView.Client.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.Cache
{
  public interface IChartDataCache
  {
    Task ClearAsync(string siteId);

    Task<IDictionary<string, ChartPowerData>> GetPowerDataAsync(string siteId);
    Task AddPowerDataAsync(string siteId, string dataId, ChartPowerData powerData);
    Task RemovePowerDataAsync(string siteId, string dataId);

    Task<IDictionary<string, DescriptorData>> GetChartDescriptorDataAsync(string siteId);
    Task AddChartDescriptorDataAsync(string siteId, string chartId, DescriptorData descriptorData);
    Task RemoveChartDescriptorDataAsync(string siteId, string chartId);
  }
}