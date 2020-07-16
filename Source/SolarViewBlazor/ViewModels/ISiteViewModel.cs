using SolarView.Common.Models;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public interface ISiteViewModel
  {
    ISiteDetails CurrentSite { get; }

    Task LoadCurrentSite(bool refresh);
    Task RefreshCurrentSite();
    Task<bool> ChangeSite(string siteId);
    Task ForgetSiteAsync();
  }
}