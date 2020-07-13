using AllOverIt.Helpers;
using Blazored.LocalStorage;
using SolarView.Client.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewBlazor.Cache
{
  public class ChartDataCache : IChartDataCache
  {
    private const string DataIndexKey = "DataIdx";
    private const string ChartIndexKey = "DescriptorIdx";
    private readonly ILocalStorageService _localStorage;
    private IList<string> _chartIds;    // don't use this explicitly - use GetChartIds()
    private IList<string> _dataIds;     // don't use this explicitly - use GetDataIds()

    public ChartDataCache(ILocalStorageService localStorage)
    {
      _localStorage = localStorage.WhenNotNull(nameof(localStorage));
    }

    public async Task ClearAsync(string siteId)
    {
      await RemoveAllPowerData(siteId).ConfigureAwait(false);
      await RemoveAllDescriptorData(siteId).ConfigureAwait(false);
      await _localStorage.ClearAsync().ConfigureAwait(false);
    }

    public async Task<IDictionary<string, ChartPowerData>> GetPowerDataAsync(string siteId)
    {
      var dataIds = await GetDataIds(siteId).ConfigureAwait(false);

      var powerData = new Dictionary<string, ChartPowerData>();

      foreach (var dataId in dataIds)
      {
        var dataIndexKey = GetDataIndexKey(siteId, dataId);
        var chartPowerData = await _localStorage.GetItemAsync<ChartPowerData>(dataIndexKey).ConfigureAwait(false);

        powerData.Add(dataId, chartPowerData);
      }

      return powerData;
    }

    public async Task AddPowerData(string siteId, string dataId, ChartPowerData powerData)
    {
      // update the individual power data
      var dataIndexKey = GetDataIndexKey(siteId, dataId);
      await _localStorage.SetItemAsync(dataIndexKey, powerData).ConfigureAwait(false);

      // update the list of data Ids
      var dataIds = await GetDataIds(siteId).ConfigureAwait(false);
      dataIds.Add(dataId);

      dataIndexKey = GetDataIndexKey(siteId);
      await _localStorage.SetItemAsync(dataIndexKey, dataIds).ConfigureAwait(false);
    }

    public async Task RemovePowerData(string siteId, string dataId)
    {
      var dataIndexKey = GetDataIndexKey(siteId, dataId);
      await _localStorage.RemoveItemAsync(dataIndexKey).ConfigureAwait(false);

      var dataIds = await GetDataIds(siteId).ConfigureAwait(false);
      dataIds.Remove(dataId);

      dataIndexKey = GetDataIndexKey(siteId);
      await _localStorage.SetItemAsync(dataIndexKey, dataIds).ConfigureAwait(false);
    }

    public async Task<IDictionary<string, DescriptorData>> GetChartDescriptorDataAsync(string siteId)
    {
      var chartIds = await GetChartIds(siteId).ConfigureAwait(false);

      var descriptorData = new Dictionary<string, DescriptorData>();

      foreach (var chartId in chartIds)
      {
        var chartIndexKey = GetChartIndexKey(siteId, chartId);
        var chartDescriptor = await _localStorage.GetItemAsync<DescriptorData>(chartIndexKey).ConfigureAwait(false);

        descriptorData.Add(chartId, chartDescriptor);
      }

      return descriptorData;
    }

    public async Task AddChartDescriptorData(string siteId, string chartId, DescriptorData descriptorData)
    {
      // update the individual descriptor data
      var chartIndexKey = GetChartIndexKey(siteId, chartId);
      await _localStorage.SetItemAsync(chartIndexKey, descriptorData).ConfigureAwait(false);

      // update the list of chart Ids
      var chartIds = await GetChartIds(siteId).ConfigureAwait(false);
      chartIds.Add(chartId);

      chartIndexKey = GetChartIndexKey(siteId);
      await _localStorage.SetItemAsync(chartIndexKey, chartIds).ConfigureAwait(false);
    }

    public async Task RemoveChartDescriptorData(string siteId, string chartId)
    {
      var chartIndexKey = GetChartIndexKey(siteId, chartId);
      await _localStorage.RemoveItemAsync(chartIndexKey).ConfigureAwait(false);

      var chartIds = await GetChartIds(siteId).ConfigureAwait(false);
      chartIds.Remove(chartId);

      chartIndexKey = GetChartIndexKey(siteId);
      await _localStorage.SetItemAsync(chartIndexKey, chartIds).ConfigureAwait(false);
    }

    private static string GetDataIndexKey(string siteId)
    {
      // key for a list of data Id's
      return $"{DataIndexKey}:{siteId}";
    }

    private static string GetDataIndexKey(string siteId, string dataId)
    {
      // key for power data against a given data Id
      return $"{DataIndexKey}:{siteId}:{dataId}";
    }

    private async Task<IList<string>> GetDataIds(string siteId)
    {
      var dataIndexKey = GetDataIndexKey(siteId);

      if (!await _localStorage.ContainKeyAsync(dataIndexKey).ConfigureAwait(false))
      {
        // can't send back a static list because the caller might add to it, resulting in it no
        // longer being empty for the next time it is required
        return new List<string>();
      }

      _dataIds ??= await _localStorage.GetItemAsync<IList<string>>(dataIndexKey).ConfigureAwait(false);

      return _dataIds;
    }

    private static string GetChartIndexKey(string siteId)
    {
      // key for a list of chart Id's
      return $"{ChartIndexKey}:{siteId}";
    }

    private static string GetChartIndexKey(string siteId, string chartId)
    {
      // key for descriptor data against a given chart Id
      return $"{ChartIndexKey}:{siteId}:{chartId}";
    }

    private async Task<IList<string>> GetChartIds(string siteId)
    {
      var chartIndexKey = GetChartIndexKey(siteId);

      if (!await _localStorage.ContainKeyAsync(chartIndexKey).ConfigureAwait(false))
      {
        // can't send back a static list because the caller might add to it, resulting in it no
        // longer being empty for the next time it is required
        return new List<string>();
      }

      _chartIds ??= await _localStorage.GetItemAsync<IList<string>>(chartIndexKey).ConfigureAwait(false);

      return _chartIds;
    }

    private async Task RemoveAllPowerData(string siteId)
    {
      var dataIds = await GetDataIds(siteId).ConfigureAwait(false);

      var tasks = dataIds.Select(dataId => RemovePowerData(siteId, dataId));
      await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task RemoveAllDescriptorData(string siteId)
    {
      var chartIds = await GetChartIds(siteId).ConfigureAwait(false);

      var tasks = chartIds.Select(chartId => RemoveChartDescriptorData(siteId, chartId));
      await Task.WhenAll(tasks).ConfigureAwait(false);
    }
  }
}