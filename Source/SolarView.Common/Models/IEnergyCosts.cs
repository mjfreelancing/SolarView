namespace SolarView.Common.Models
{
  public interface IEnergyCosts
  {
    double CostPerKWhPeak { get; }
    double CostPerKWhOffPeak { get; }
    double SolarBuyBack { get; }
  }
}