using AllOverIt.Helpers;
using Blazored.LocalStorage;
using SolarViewBlazor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewBlazor.Cache
{
  public class ChartDataCache : IChartDataCache
  {
    private const string IndexIdKey = "ChartIdx";
    private const string ChartPrefixKey = "Chart";
    private static readonly IList<string> EmptyChartIds = new List<string>();
    private readonly ILocalStorageService _localStorage;
    private IList<string> _chartIds;    // don't use this explicitly - use GetChartIds()

    public ChartDataCache(ILocalStorageService localStorage)
    {
      _localStorage = localStorage.WhenNotNull(nameof(localStorage));
    }

    public async Task<int> GetCount(string siteId)
    {
      var chartIds = await GetChartIds(siteId);

      return chartIds.Count;
    }

    public async Task<IReadOnlyList<ChartData>> GetData(string siteId)
    {
      var chartIds = await GetChartIds(siteId).ConfigureAwait(false);

      var chartTasks = chartIds.Select(chartId => _localStorage.GetItemAsync<ChartData>(GetChartIndexKey(siteId, chartId)));

      var results = await Task.WhenAll(chartTasks).ConfigureAwait(false);

      // can't just return 'results' because the calling code needs a non-fixed size collection
      return results.ToList();
    }

    public async Task Add(string siteId, ChartData chartData)
    {
      var siteChartIndexKey = GetChartIndexKey(siteId, chartData.Id);
      await _localStorage.SetItemAsync(siteChartIndexKey, chartData);

      var chartIds = await GetChartIds(siteId).ConfigureAwait(false);

      chartIds.Add(chartData.Id);

      var siteIdIndexKey = GetSiteIdIndexKey(siteId);
      await _localStorage.SetItemAsync(siteIdIndexKey, chartIds).ConfigureAwait(false);
    }

    public async Task Remove(string siteId, string chartId)
    {
      var siteChartIndexKey = GetChartIndexKey(siteId, chartId);
      await _localStorage.RemoveItemAsync(siteChartIndexKey);

      var chartIds = await GetChartIds(siteId).ConfigureAwait(false);
      chartIds.Remove(chartId);

      var siteIdIndexKey = GetSiteIdIndexKey(siteId);
      await _localStorage.SetItemAsync(siteIdIndexKey, chartIds).ConfigureAwait(false);
    }

    private async Task<IList<string>> GetChartIds(string siteId)
    {
      var siteIdIndexKey = GetSiteIdIndexKey(siteId);

      if (!await _localStorage.ContainKeyAsync(siteIdIndexKey).ConfigureAwait(false))
      {
        return EmptyChartIds;
      }

      _chartIds ??= await _localStorage.GetItemAsync<IList<string>>(siteIdIndexKey);

      return _chartIds;
    }

    private static string GetChartIndexKey(string siteId, string chartId)
    {
      return $"{ChartPrefixKey}:{siteId}:{chartId}";
    }

    private static string GetSiteIdIndexKey(string siteId)
    {
      return $"{IndexIdKey}:{siteId}";
    }
  }
}