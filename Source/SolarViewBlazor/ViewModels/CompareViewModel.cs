using AllOverIt.Extensions;
using AllOverIt.Helpers;
using SolarView.Client.Common.Models;
using SolarView.Client.Common.Services.Site;
using SolarView.Client.Common.Services.SolarView;
using SolarView.Common.Models;
using SolarViewBlazor.Cache;
using SolarViewBlazor.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public class CompareViewModel : ICompareViewModel
  {
    private bool _cacheIsLoaded;
    private ISiteDetails _currentSite;
    private readonly ISiteService _siteService;
    private readonly ISolarViewService _solarViewService;
    private readonly IChartRegistry _chartRegistry;
    private readonly IChartDataCache _chartDataCache;

    // groups of chart descriptors used to build the different chart types, mapped to one or more date ranges and associated power data
    private readonly IDictionary<IChartDescriptor, IList<ChartData>> _chartsToRender = new Dictionary<IChartDescriptor, IList<ChartData>>();

    // a list of all unique date ranges and associated power data (key is a unique Id associated with the data)
    private IDictionary<string, ChartPowerData> _chartPowerData; // = new Dictionary<string, ChartPowerData>();

    // a collection of all charts (key is the chart Id) and their associated descriptor / data
    private IDictionary<string, DescriptorData> _chartDescriptorData = new Dictionary<string, DescriptorData>();

    public CompareViewModel(ISiteService siteService, ISolarViewService solarViewService, IChartRegistry chartRegistry,
      IChartDataCache chartDataCache)
    {
      _siteService = siteService.WhenNotNull(nameof(siteService));
      _solarViewService = solarViewService.WhenNotNull(nameof(solarViewService));
      _chartRegistry = chartRegistry.WhenNotNull(nameof(chartRegistry));
      _chartDataCache = chartDataCache.WhenNotNull(nameof(chartDataCache));
    }

    public async Task LoadFromCacheAsync()
    {
      if (!_cacheIsLoaded)
      {
        var currentSiteId = await GetCurrentSiteId();

        _chartPowerData = await _chartDataCache.GetPowerDataAsync(currentSiteId);
        _chartDescriptorData = await _chartDataCache.GetChartDescriptorDataAsync(currentSiteId);

        foreach (var (chartId, descriptorData) in _chartDescriptorData)
        {
          var chartDataId = descriptorData.ChartDataId;
          var powerData = _chartPowerData[chartDataId];

          var descriptorId = descriptorData.DescriptorId;
          var chartDescriptor = _chartRegistry.ChartDescriptors.Single(item => item.Id == descriptorId);

          var chartData = new ChartData
          {
            Id = chartId,
            DescriptorId = descriptorId,
            StartDate = powerData.StartDate,
            EndDate = powerData.EndDate,
            Power = powerData.Power
          };

          // add to the list of charts to be rendered
          UpdateChartsToRender(chartDescriptor, chartData);
        }

        _cacheIsLoaded = true;
      }
    }

    public async Task<bool> AddChartsAsync(DateRange dateRange, IEnumerable<IChartDescriptor> chartDescriptors)
    {
      var (chartDataId, cachedPowerData) = await GetChartPowerData(dateRange);

      // determine what chart(s) need to be created (avoids duplication)
      var missingDescriptors = chartDescriptors.Where(descriptor => !ChartDataExists(chartDataId, descriptor.Id)).ToList();

      if (missingDescriptors.Count == 0)
      {
        return false;
      }

      foreach (var chartDescriptor in missingDescriptors)
      {
        var chartId = $"{Guid.NewGuid()}";
        var descriptorData = new DescriptorData(chartDataId, chartDescriptor.Id);

        _chartDescriptorData.Add(chartId, descriptorData);

        var chartData = new ChartData
        {
          Id = chartId,
          DescriptorId = chartDescriptor.Id,
          StartDate = dateRange.StartDateTime,
          EndDate = dateRange.EndDateTime,
          Power = cachedPowerData.Power
        };

        // add to the list of charts to be rendered
        UpdateChartsToRender(chartDescriptor, chartData);

        // Add the data to the cache
        var currentSiteId = await GetCurrentSiteId();
        await _chartDataCache.AddChartDescriptorDataAsync(currentSiteId, chartId, descriptorData);
      }

      return true;
    }

    public async Task DeleteChartAsync(string chartId)
    {
      // get the Id of the data and descriptor associated with this chart
      var descriptorData = _chartDescriptorData[chartId];

      // get the chart descriptor
      var chartDescriptor = _chartsToRender
        .Single(item => item.Key.Id == descriptorData.DescriptorId)
        .Key;

      // get all data associated with the descriptor (multiple charts)
      var descriptorChartData = _chartsToRender[chartDescriptor];

      // remove the data associated with this chart
      var chartData = descriptorChartData.Single(item => item.Id == chartId);
      descriptorChartData.Remove(chartData);

      // if there's no more charts associated with this descriptor then we can remove the entire group
      if (descriptorChartData.Count == 0)
      {
        _chartsToRender.Remove(chartDescriptor);
      }

      // remove the chart reference
      _chartDescriptorData.Remove(chartId);

      var currentSiteId = await GetCurrentSiteId();

      // remove from the cache
      await _chartDataCache.RemoveChartDescriptorDataAsync(currentSiteId, chartId);

      // if there are no charts referring to the same power data then it can be removed
      if (_chartDescriptorData.Values.All(item => item.ChartDataId != descriptorData.ChartDataId))
      {
        _chartPowerData.Remove(descriptorData.ChartDataId);

        // remove from the cache
        await _chartDataCache.RemovePowerDataAsync(currentSiteId, descriptorData.ChartDataId);
      }
    }

    public IReadOnlyList<IChartDescriptor> GetDescriptors()
    {
      // make sure the view adds them in a consistent order
      return _chartsToRender.Keys
        .OrderBy(item => item.Description)
        .AsReadOnlyList();
    }

    public IReadOnlyList<ChartData> GetDescriptorData(IChartDescriptor descriptor)
    {
      return _chartsToRender[descriptor].AsReadOnlyList();
    }
    private async Task<string> GetCurrentSiteId()
    {
      _currentSite ??= await _siteService.GetCurrentSite();

      return _currentSite?.SiteId;
    }

    private bool ChartDataExists(string chartDataId, string chartDescriptorId)
    {
      return _chartDescriptorData.Values.Any(item => item.ChartDataId == chartDataId &&
                                                     item.DescriptorId == chartDescriptorId);
    }

    private void UpdateChartsToRender(IChartDescriptor chartDescriptor, ChartData chartData)
    {
      // determine if any of the existing charts are of the same required type
      if (_chartsToRender.Keys.All(item => item.Id != chartData.DescriptorId))
      {
        _chartsToRender.Add(chartDescriptor, new List<ChartData>());
      }

      // track the data associated with the required chart descriptor
      _chartsToRender[chartDescriptor].Add(chartData);
    }

    private async Task<(string chartDataId, ChartPowerData cachedPowerData)> GetChartPowerData(DateRange dateRange)
    {
      var startDate = dateRange.StartDateTime;
      var endDate = dateRange.EndDateTime;

      // check if we already have the required data
      var cachedPowerDataItems = _chartPowerData
        .Where(kvp => kvp.Value.StartDate == startDate && kvp.Value.EndDate == endDate)
        .ToList();

      string chartDataId;
      ChartPowerData cachedPowerData;

      if (cachedPowerDataItems.Any())
      {
        (chartDataId, cachedPowerData) = cachedPowerDataItems.First();
      }
      else
      {
        var currentSiteId = await GetCurrentSiteId();

        var powerData = (await _solarViewService.GetPowerData(currentSiteId, startDate, endDate)).AsReadOnlyList();

        chartDataId = $"{Guid.NewGuid()}";
        cachedPowerData = new ChartPowerData(startDate, endDate, powerData);

        _chartPowerData.Add(chartDataId, cachedPowerData);

        await _chartDataCache.AddPowerDataAsync(currentSiteId, chartDataId, cachedPowerData);
      }

      return (chartDataId, cachedPowerData);
    }
  }
}