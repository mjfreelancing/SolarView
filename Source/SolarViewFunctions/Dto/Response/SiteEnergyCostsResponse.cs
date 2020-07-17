using SolarView.Common.Models;

namespace SolarViewFunctions.Dto.Response
{
  public class SiteEnergyCostsResponse : ISiteEnergyCosts
  {
    public string SiteId { get; set; }
    public double OffPeakRate { get; set; }               // per KWh (assumes 10pm to 7am)
    public double PeakRate { get; set; }                  // per KWh (assumes Block1 and Block2 are the same rate)
    public double SupplyCharge { get; set; }              // per day
    public double SolarBuyBackRate { get; set; }          // per KWh
  }
}