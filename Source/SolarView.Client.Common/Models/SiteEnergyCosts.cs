using SolarView.Common.Models;

namespace SolarView.Client.Common.Models
{
  public class SiteEnergyCosts : ISiteEnergyCosts
  {
    public string SiteId { get; set; }
    public string StartDate { get; set; }
    public double OffPeakRate { get; set; }
    public double PeakRate { get; set; }
    public double SupplyCharge { get; set; }
    public double SolarBuyBackRate { get; set; }

    public SiteEnergyCosts()
    {
    }

    public SiteEnergyCosts(string siteId)
    {
      SiteId = siteId;
    }
  }
}