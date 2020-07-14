using SolarView.Common.Models;

namespace SolarViewBlazor.Messaging
{


  public class SiteChanged 
  {
    public ISiteInfo SiteInfo { get; }

    public SiteChanged(ISiteInfo siteInfo)
    {
      SiteInfo = siteInfo;
    }
  }

 
}