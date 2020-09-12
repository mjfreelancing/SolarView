using SolarView.Common.Models;

namespace SolarViewBlazor.Messaging
{
  public sealed class SiteChanged 
  {
    public ISiteDetails SiteDetails { get; }

    public SiteChanged(ISiteDetails siteDetails)
    {
      SiteDetails = siteDetails;
    }
  }
}