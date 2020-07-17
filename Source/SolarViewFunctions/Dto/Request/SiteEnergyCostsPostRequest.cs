using SolarView.Common.Models;

namespace SolarViewFunctions.Dto.Request
{
  public class SiteEnergyCostsPostRequest : ISiteEnergyCosts
  {
    public string SiteId { get; set; }
    public double OffPeakRate { get; set; }
    public double PeakRate { get; set; }
    public double SupplyCharge { get; set; }
    public double SolarBuyBackRate { get; set; }
  }
}