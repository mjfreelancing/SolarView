using AllOverIt.Helpers;
using SolarView.Client.Common.Services.SolarView;
using SolarView.Common.Models;
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

    public Task<ISiteEnergyCosts> GetEnergyCosts(string siteId)
    {
      return _solarViewService.GetEnergyCosts(siteId);
    }

    public Task UpdateEnergyCosts(ISiteEnergyCosts energyCosts)
    {
      return _solarViewService.UpsertEnergyCosts(energyCosts);
    }
  }
}