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

    public Task<IEnergyCosts> GetEnergyCosts(string siteId)
    {
      return _solarViewService.GetEnergyCosts(siteId);
    }
  }
}