using SolarView.Common.Models;

namespace SolarViewFunctions.Dto.Request
{
  public class SiteEnergyCostsPostRequest : ISiteEnergyCosts
  {
    public string SiteId { get; set; }
    public double CostPerKWhPeak { get; set; }
    public double CostPerKWhOffPeak { get; set; }
    public double SolarBuyBack { get; set; }
  }
}