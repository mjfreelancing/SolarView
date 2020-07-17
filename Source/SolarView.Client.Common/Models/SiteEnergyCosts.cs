using SolarView.Common.Models;

namespace SolarView.Client.Common.Models
{
  public class SiteEnergyCosts : ISiteEnergyCosts
  {
    public string SiteId { get; set; }
    public double CostPerKWhPeak { get; set; }
    public double CostPerKWhOffPeak { get; set; }
    public double SolarBuyBack { get; set; }

    public SiteEnergyCosts()
    {
    }

    public SiteEnergyCosts(string siteId)
    {
      SiteId = siteId;
    }
  }
}