using SolarView.Common.Models;

namespace SolarViewBlazor.Messaging
{


  public class SiteChanged 
  {
    public ISiteDetails SiteDetails { get; }

    public SiteChanged(ISiteDetails siteDetails)
    {
      SiteDetails = siteDetails;
    }
  }

 
}