namespace SolarView.Common.Models
{
  public interface ISiteEnergyCosts
  {
    string SiteId { get; }
    double CostPerKWhPeak { get; }
    double CostPerKWhOffPeak { get; }
    double SolarBuyBack { get; }
  }
}