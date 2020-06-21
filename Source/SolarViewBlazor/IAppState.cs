using SolarView.Common.Models;
using System;

namespace SolarViewBlazor
{
  public interface IAppState
  {
    ISiteInfo CurrentSite { get; set; }

    event Action<ISiteInfo> OnSiteChanged;
  }
}