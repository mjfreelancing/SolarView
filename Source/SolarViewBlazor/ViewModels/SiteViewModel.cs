using AllOverIt.Helpers;
using SolarView.Client.Common.Services.Site;
using SolarView.Client.Common.Services.SolarView;
using SolarView.Common.Models;
using SolarViewBlazor.Events;
using SolarViewBlazor.Messaging;
using System;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public class SiteViewModel : ISiteViewModel
  {
    private readonly IEventAggregator _eventAggregator;
    private readonly ISiteService _siteService;
    private readonly ISolarViewService _solarViewService;

    public ISiteDetails CurrentSite { get; private set; }

    public SiteViewModel(ISiteService siteService, ISolarViewService solarViewService, IEventAggregator eventAggregator)
    {
      _siteService = siteService.WhenNotNull(nameof(siteService));
      _solarViewService = solarViewService.WhenNotNull(nameof(solarViewService));
      _eventAggregator = eventAggregator.WhenNotNull(nameof(eventAggregator));
    }

    public async Task LoadCurrentSite(bool refresh)
    {
      CurrentSite = await _siteService.GetCurrentSite();

      // CurrentSite will be null if the cache is clear
      if (refresh && CurrentSite != null)
      {
        await RefreshCurrentSite();
      }
    }

    public async Task RefreshCurrentSite()
    {
      if (CurrentSite == null)
      {
        throw new InvalidOperationException("There is no current site to refresh");
      }

      await ChangeSite(CurrentSite.SiteId);
    }

    public async Task<bool> ChangeSite(string siteId)
    {
      var siteInfo = await _solarViewService.GetSiteDetails(siteId);

      if (siteInfo == null)
      {
        return false;
      }

      CurrentSite = siteInfo;
      await CurrentSiteChanged();

      return true;
    }

    public Task ForgetSiteAsync()
    {
      CurrentSite = null;
      return CurrentSiteChanged();
    }

    private async Task CurrentSiteChanged()
    {
      await _siteService.SetCurrentSite(CurrentSite);
      await _eventAggregator.PublishAsync(new SiteChanged(CurrentSite));
    }
  }
}