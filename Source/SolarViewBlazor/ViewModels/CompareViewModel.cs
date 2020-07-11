using AllOverIt.Extensions;
using AllOverIt.Helpers;
using SolarView.Client.Common.Services.SolarView;
using SolarView.Common.Models;
using SolarViewBlazor.Cache;
using SolarViewBlazor.Charts;
using SolarViewBlazor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public class CompareViewModel : ICompareViewModel
  {
    private readonly ISiteViewModel _siteViewModel;
    private readonly ISolarViewService _solarViewService;
    private readonly IChartDataCache _chartDataCache;

    // groups of chart descriptors used to build the different chart types, mapped to one or more date ranges and associated power data
    private readonly IDictionary<IChartDescriptor, IList<ChartData>> _chartsToRender = new Dictionary<IChartDescriptor, IList<ChartData>>();

    // a list of all unique date ranges and associated power data (key is a unique Id associated with the data)
    private readonly IDictionary<string, ChartPowerData> _chartPowerData = new Dictionary<string, ChartPowerData>();

    // a collection of all charts (key is the chart Id) and their associated descriptor / data
    private readonly IDictionary<string, DescriptorData> _chartDescriptorData = new Dictionary<string, DescriptorData>();

    public CompareViewModel(ISiteViewModel siteViewModel, ISolarViewService solarViewService, IChartDataCache chartDataCache)
    {
      _siteViewModel = siteViewModel.WhenNotNull(nameof(siteViewModel));
      _solarViewService = solarViewService.WhenNotNull(nameof(solarViewService));
      _chartDataCache = chartDataCache.WhenNotNull(nameof(chartDataCache));
    }

    public async Task<bool> AddCharts(DateRange dateRange, IEnumerable<IChartDescriptor> chartDescriptors)
    {
      var (chartDataId, cachedPowerData) = await GetChartPowerData(dateRange);

      // determine what chart(s) need to be created (avoids duplication)
      var missingDescriptors = chartDescriptors.Where(item => !ChartExists(chartDataId, item.Id)).ToList();

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
        AddChartDescriptor(chartDescriptor, chartData);

        // Add the data to the cache
        //await _chartDataCache.Add(SiteViewModel.CurrentSite.SiteId, chartData);
      }

      return true;
    }

    public Task DeleteChart(string chartId)
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

      // if there are no charts referring to the same power data then it can be removed
      if (_chartDescriptorData.Values.All(item => item.ChartDataId != descriptorData.ChartDataId))
      {
        _chartPowerData.Remove(descriptorData.ChartDataId);
      }

      // remove from the cache
      //await _chartDataCache.Remove(SiteViewModel.CurrentSite.SiteId, chartId);

      return Task.CompletedTask;
    }

    public IEnumerable<IChartDescriptor> GetDescriptors()
    {
      return _chartsToRender.Keys;
    }

    public IEnumerable<ChartData> GetChartData(IChartDescriptor descriptor)
    {
      return _chartsToRender[descriptor];
    }

    private bool ChartExists(string chartDataId, string chartDescriptorId)
    {
      return _chartDescriptorData.Values.Any(item => item.ChartDataId == chartDataId &&
                                                     item.DescriptorId == chartDescriptorId);
    }

    private void AddChartDescriptor(IChartDescriptor chartDescriptor, ChartData chartData)
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
        .Where(kvp => kvp.Value.StartDate == startDate &&
                      kvp.Value.EndDate == endDate)
        .ToList();

      string chartDataId;
      ChartPowerData cachedPowerData;

      if (cachedPowerDataItems.Any())
      {
        (chartDataId, cachedPowerData) = cachedPowerDataItems.First();
      }
      else
      {
        var powerData = (await _solarViewService.CollectData(_siteViewModel.CurrentSite.SiteId, startDate, endDate)).AsReadOnlyList();

        chartDataId = $"{Guid.NewGuid()}";
        cachedPowerData = new ChartPowerData(startDate, endDate, powerData);

        _chartPowerData.Add(chartDataId, cachedPowerData);
      }

      return (chartDataId, cachedPowerData);
    }
  }
}