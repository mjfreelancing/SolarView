using AllOverIt.Helpers;
using Blazored.LocalStorage;
using SolarView.Client.Common.Services.SolarView;
using SolarView.Common.Models;
using System.Threading.Tasks;

namespace SolarViewBlazor.Models
{
  public class AppState : IAppState
  {
    private ISiteInfo _site;
    private readonly ILocalStorageService _localStorage;
    private readonly ISolarViewService _solarViewService;

    public string SiteId => _site?.SiteId;
    public string StartDate => _site?.StartDate;
    public string ContactName => _site?.ContactName;
    public string ContactEmail => _site?.ContactEmail;
    public string TimeZoneId => _site?.TimeZoneId;
    public string LastRefreshDateTime => _site?.LastRefreshDateTime;
    public string LastAggregationDate => _site?.LastAggregationDate;
    public string LastSummaryDate => _site?.LastSummaryDate;

    public AppState(ILocalStorageService localStorage, ISolarViewService solarViewService)
    {
      _localStorage = localStorage.WhenNotNull(nameof(localStorage));
      _solarViewService = solarViewService.WhenNotNull(nameof(solarViewService));
    }

    public async Task<bool> LoadLastKnownSite()
    {
      var siteId = await _localStorage.GetItemAsync<string>("CurrentSiteId");

      if (SiteId == siteId)
      {
        return false;
      }

      return await SetSiteAsync(siteId);
    }

    public async Task<bool> SetSiteAsync(string siteId)
    {
      var site = await _solarViewService.GetSiteDetails(siteId);

      if(site != null)
      {
        _site = site;
        await _localStorage.SetItemAsync("CurrentSiteId", siteId);
      }

      return _site != null;
    }
  }
}