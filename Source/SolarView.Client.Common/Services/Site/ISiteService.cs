using SolarView.Common.Models;
using System.Threading.Tasks;

namespace SolarView.Client.Common.Services.Site
{
  public interface ISiteService
  {
    Task<ISiteInfo> GetCurrentSite();
    Task SetCurrentSite(ISiteInfo siteInfo);
  }
}