using AllOverIt.Extensions;
using AllOverIt.Helpers;
using AllOverIt.Tasks;
using SolarView.Client.Common.Models;
using SolarView.Client.Common.Services.SolarView;
using SolarView.Common.Models;
using SolarViewBlazor.Charts.Models;
using SolarViewBlazor.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewBlazor.Charts.ViewModels
{
  public class CostBenefitChartViewModel : ICostBenefitChartViewModel
  {
    private readonly AsyncLazy<ISiteEnergyCosts> _siteEnergyCosts;

    public CostBenefitChartViewModel(ISiteViewModel siteViewModel, ISolarViewService solarViewService)
    {
      _ = siteViewModel.WhenNotNull(nameof(siteViewModel));
      _ = solarViewService.WhenNotNull(nameof(solarViewService));

      _siteEnergyCosts = new AsyncLazy<ISiteEnergyCosts>(async () =>
        await solarViewService.GetEnergyCosts(siteViewModel.CurrentSite.SiteId));
    }

    public async Task<IReadOnlyList<PowerCost>> CalculateData(IEnumerable<PowerData> powerData, bool isCumulative)
    {
      var energyCosts = await _siteEnergyCosts;

      return isCumulative
        ? CalculateCumulativeData(powerData, energyCosts)
        : CalculateNonCumulativeData(powerData, energyCosts);
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