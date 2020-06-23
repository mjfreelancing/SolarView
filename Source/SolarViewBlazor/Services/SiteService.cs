using AllOverIt.Helpers;
using Blazored.LocalStorage;
using SolarView.Client.Common.Models;
using SolarView.Client.Common.Services.Site;
using SolarView.Common.Models;
using System;
using System.Threading.Tasks;

namespace SolarViewBlazor.Services
{
  public class SiteService : ISiteService, IDisposable
  {
    private const string SiteInfoKey = nameof(SiteInfo);
    private readonly ILocalStorageService _localStorage;
    private readonly IAppState _appState;

    public SiteService(ILocalStorageService localStorage, IAppState appState)
    {
      _localStorage = localStorage.WhenNotNull(nameof(localStorage));
      _appState = appState.WhenNotNull(nameof(appState));

      _appState.OnSiteChanged += OnSiteChanged;
    }

    public async Task<ISiteInfo> GetCurrentSite()
    {
      if (await _localStorage.ContainKeyAsync(SiteInfoKey))
      {
        return await _localStorage.GetItemAsync<SiteInfo>(SiteInfoKey);
      }

      return null;
    }

    public void Dispose()
    {
      _appState.OnSiteChanged -= OnSiteChanged;
    }

    private async void OnSiteChanged(ISiteInfo siteInfo)
    {
      if (siteInfo == null)
      {
        await _localStorage.RemoveItemAsync(SiteInfoKey);
      }
      else
      {
        if (siteInfo.GetType() != typeof(SiteInfo))
        {
          throw new InvalidOperationException($"Expected the '{nameof(siteInfo)}' to be of type '{nameof(SiteInfo)}'");
        }

        // note: cannot just pass 'siteInfo' - will not be serialized correctly
        await _localStorage.SetItemAsync(SiteInfoKey, siteInfo as SiteInfo);
      }
    }
  }
}