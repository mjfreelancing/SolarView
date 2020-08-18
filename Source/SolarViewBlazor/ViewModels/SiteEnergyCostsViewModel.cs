using AllOverIt.Helpers;
using SolarView.Client.Common.Services.SolarView;
using SolarView.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public class SiteEnergyCostsViewModel : ISiteEnergyCostsViewModel
  {
    private readonly ISolarViewService _solarViewService;

    public SiteEnergyCostsViewModel(ISolarViewService solarViewService)
    {
      _solarViewService = solarViewService.WhenNotNull(nameof(solarViewService));
    }

    // todo: these costs need to be date based
    public Task<IReadOnlyList<ISiteEnergyCosts>> GetEnergyCosts(string siteId)
    {
      return _solarViewService.GetEnergyCosts(siteId);
    }

    public Task UpdateEnergyCosts(ISiteEnergyCosts energyCosts)
    {
      return _solarViewService.UpsertEnergyCosts(energyCosts);
    }
  }
}