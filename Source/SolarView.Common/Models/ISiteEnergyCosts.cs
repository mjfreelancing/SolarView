namespace SolarView.Common.Models
{
  public interface ISiteEnergyCosts
  {
    string SiteId { get; }
    string StartDate { get; }                 // yyyyMMdd
    double OffPeakRate { get; }               // per KWh (assumes 10pm to 7am) - optional since cannot easily determine portion of power using off-peak circuit
    double PeakRate { get; }                  // per KWh (assumes Block1 and Block2 are the same rate)
    double SupplyCharge { get; }              // per day
    double SolarBuyBackRate { get; }          // per KWh
  }
}