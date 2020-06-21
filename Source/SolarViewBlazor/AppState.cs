using AllOverIt.Extensions;
using SolarView.Common.Models;
using System;

namespace SolarViewBlazor
{
  public class AppState : IAppState
  {
    private ISiteInfo _siteInfo;

    public event Action<ISiteInfo> OnSiteChanged;

    public ISiteInfo CurrentSite
    {
      get => _siteInfo;
      set
      {
        // comparing all properties in case a refresh was performed
        if (_siteInfo?.CalculateHashCode() != value?.CalculateHashCode())
        {
          _siteInfo = value;
          OnSiteChanged?.Invoke(_siteInfo);
        }
      }
    }
  }
}