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
    private const string IndexIdKey = "ChartIndexIds";
    private const string ChartPrefixKey = "ChartId";
    private static readonly IList<string> EmptyChartIds = new List<string>();
    private readonly ILocalStorageService _localStorage;
    private IList<string> _chartIds;    // don't use this explicitly - use GetChartIds()

    public ChartDataCache(ILocalStorageService localStorage)
    {
      _localStorage = localStorage.WhenNotNull(nameof(localStorage));
    }

    public async Task<int> GetCount()
    {
      var chartIds = await GetChartIds();

      return chartIds.Count;
    }

    public async Task<IList<ChartData>> GetData()
    {
      var chartIds = await GetChartIds().ConfigureAwait(false);

      var chartTasks = chartIds.Select(chartId => _localStorage.GetItemAsync<ChartData>(GetChartIndexKey(chartId)));

      var results = await Task.WhenAll(chartTasks).ConfigureAwait(false);

      // can't just return 'results' because the calling code needs a non-fixed size collection
      return results.ToList();
    }

    public async Task Add(ChartData chartData)
    {
      await _localStorage.SetItemAsync(GetChartIndexKey(chartData.Id), chartData);

      var chartIds = await GetChartIds().ConfigureAwait(false);

      chartIds.Add(chartData.Id);
      await _localStorage.SetItemAsync(IndexIdKey, chartIds).ConfigureAwait(false);
    }

    public async Task Remove(string chartId)
    {
      await _localStorage.RemoveItemAsync(GetChartIndexKey(chartId));

      var chartIds = await GetChartIds().ConfigureAwait(false);

      chartIds.Remove(chartId);
      await _localStorage.SetItemAsync(IndexIdKey, chartIds).ConfigureAwait(false);
    }

    private async Task<IList<string>> GetChartIds()
    {
      if (!await _localStorage.ContainKeyAsync(IndexIdKey).ConfigureAwait(false))
      {
        return EmptyChartIds;
      }

      _chartIds ??= await _localStorage.GetItemAsync<IList<string>>(IndexIdKey);

      return _chartIds;
    }

    private static string GetChartIndexKey(string chartId)
    {
      return $"{ChartPrefixKey}:{chartId}";
    }
  }
}