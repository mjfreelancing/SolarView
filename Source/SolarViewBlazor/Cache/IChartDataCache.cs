using SolarViewBlazor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.Cache
{
  public interface IChartDataCache
  {
    Task ClearAsync(string siteId);

    Task<IDictionary<string, ChartPowerData>> GetPowerDataAsync(string siteId);
    Task AddPowerData(string siteId, string dataId, ChartPowerData powerData);
    Task RemovePowerData(string siteId, string dataId);

    Task<IDictionary<string, DescriptorData>> GetChartDescriptorDataAsync(string siteId);
    Task AddChartDescriptorData(string siteId, string chartId, DescriptorData descriptorData);
    Task RemoveChartDescriptorData(string siteId, string chartId);
  }
}