using System.Threading.Tasks;
using SolarView.Common.Models;

namespace SolarViewBlazor.ViewModels
{
  public interface ISiteViewModel
  {
    ISiteInfo CurrentSite { get; }

    Task LoadCurrentSite(bool refresh);
    Task RefreshCurrentSite();
    Task<bool> ChangeSite(string siteId);
    Task ForgetSiteAsync();
  }
}