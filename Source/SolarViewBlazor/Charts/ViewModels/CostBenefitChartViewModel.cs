using AllOverIt.Extensions;
using AllOverIt.Helpers;
using AllOverIt.Tasks;
using SolarView.Client.Common.Models;
using SolarView.Client.Common.Services.Site;
using SolarView.Client.Common.Services.SolarView;
using SolarView.Common.Models;
using SolarViewBlazor.Charts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewBlazor.Charts.ViewModels
{
  public class CostBenefitChartViewModel : ICostBenefitChartViewModel
  {
    private readonly AsyncLazy<IReadOnlyList<ISiteEnergyCosts>> _siteEnergyCosts;

    public CostBenefitChartViewModel(ISiteService siteService, ISolarViewService solarViewService)
    {
      _ = siteService.WhenNotNull(nameof(siteService));
      _ = solarViewService.WhenNotNull(nameof(solarViewService));

      _siteEnergyCosts = new AsyncLazy<IReadOnlyList<ISiteEnergyCosts>>(async () =>
      {
        var currentSite = await siteService.GetCurrentSite();
        return await solarViewService.GetEnergyCosts(currentSite.SiteId);
      });
    }

    public async Task<IReadOnlyList<PowerCost>> CalculateData(IEnumerable<PowerData> powerData, DateTime startDate, DateTime endDate,
      bool isCumulative)
    {
      var energyCosts = (await _siteEnergyCosts)
        .OrderByDescending(item => item.StartDate)
        .AsReadOnlyList();

      // calculate average costs over the required period - the SiteId and StartDate are not relevant for the calculations performed here
      var dayCount = (endDate - startDate).Days + 1;
      var dailyCosts = new SiteEnergyCosts();
      var costsEndDate = DateTime.MaxValue;

      foreach (var cost in energyCosts)
      {
        var costsStartDate = DateTime.ParseExact(cost.StartDate, "yyyyMMdd", null);

        var overlappingDays = GetOverlappingDays(startDate, endDate, costsStartDate, costsEndDate);

        if (overlappingDays > 0)
        {
          costsEndDate = costsStartDate.AddDays(-1);

          dailyCosts.OffPeakRate += overlappingDays * cost.OffPeakRate / dayCount;
          dailyCosts.PeakRate += overlappingDays * cost.PeakRate / dayCount;
          dailyCosts.SolarBuyBackRate += overlappingDays * cost.SolarBuyBackRate / dayCount;
          dailyCosts.SupplyCharge += overlappingDays * cost.SupplyCharge / dayCount;
        }
      }

      return isCumulative
        ? CalculateCumulativeData(powerData, dailyCosts)
        : CalculateNonCumulativeData(powerData, dailyCosts);
    }

    private static double GetOverlappingDays(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
    {
      var maxStart = firstStart > secondStart ? firstStart : secondStart;
      var minEnd = firstEnd < secondEnd ? firstEnd : secondEnd;
      var interval = minEnd - maxStart;

      return interval >= TimeSpan.FromSeconds(0) ? interval.TotalDays + 1 : 0;
    }

    private static IReadOnlyList<PowerCost> CalculateCumulativeData(IEnumerable<PowerData> powerData, ISiteEnergyCosts siteEnergyCosts)
    {
      var lastPowerCost = new PowerCost();

      return powerData
        .Select(item =>
        {
                // cost for the current data point
                var powerCost = new PowerCost(item, siteEnergyCosts);

                // add previous aggregation
                powerCost.AddCost(lastPowerCost);

          lastPowerCost = powerCost;

          return powerCost;
        })
        .AsReadOnlyList();
    }

    private static IReadOnlyList<PowerCost> CalculateNonCumulativeData(IEnumerable<PowerData> powerData, ISiteEnergyCosts siteEnergyCosts)
    {
      return powerData
        .Select(item => new PowerCost(item, siteEnergyCosts))
        .AsReadOnlyList();
    }
  }
}