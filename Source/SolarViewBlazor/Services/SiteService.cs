using AllOverIt.Helpers;
using Blazored.LocalStorage;
using SolarView.Client.Common.Models;
using SolarView.Client.Common.Services.Site;
using SolarView.Common.Models;
using System;
using System.Threading.Tasks;

namespace SolarViewBlazor.Services
{
  public class SiteService : ISiteService
  {
    private const string SiteInfoKey = nameof(SiteDetails);
    private readonly ILocalStorageService _localStorage;

    public SiteService(ILocalStorageService localStorage)
    {
      _localStorage = localStorage.WhenNotNull(nameof(localStorage));
    }

    public async Task<ISiteDetails> GetCurrentSite()
    {
      if (await _localStorage.ContainKeyAsync(SiteInfoKey))
      {
        return await _localStorage.GetItemAsync<SiteDetails>(SiteInfoKey);
      }

      return null;
    }

    public async Task SetCurrentSite(ISiteDetails siteDetails)
    {
      if (siteDetails == null)
      {
        await _localStorage.RemoveItemAsync(SiteInfoKey);
      }
      else
      {
        if (siteDetails.GetType() != typeof(SiteDetails))
        {
          throw new InvalidOperationException($"Expected the '{nameof(siteDetails)}' to be of type '{nameof(SiteDetails)}'");
        }

        // note: cannot just pass 'siteInfo' - will not be serialized correctly
        await _localStorage.SetItemAsync(SiteInfoKey, siteDetails as SiteDetails);
      }
    }
  }
}