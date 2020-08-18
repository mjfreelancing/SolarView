namespace SolarViewFunctions.Dto.Response
{
  public class SiteEnergyCostsResponse //: ISiteEnergyCosts
  {
    public string SiteId { get; set; }
    public string StartDate { get; set; }                 // yyyyMMdd
    public double OffPeakRate { get; set; }               // per KWh (assumes 10pm to 7am, when used)
    public double PeakRate { get; set; }                  // per KWh (assumes all blocks are the same rate)
    public double SupplyCharge { get; set; }              // per day
    public double SolarBuyBackRate { get; set; }          // per KWh
  }
}