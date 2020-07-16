using SolarView.Common.Models;

namespace SolarViewFunctions.Dto.Response
{
  public class SiteEnergyCostsResponse : IEnergyCosts
  {
    public double CostPerKWhPeak { get; set; }
    public double CostPerKWhOffPeak { get; set; }
    public double SolarBuyBack { get; set; }
  }
}