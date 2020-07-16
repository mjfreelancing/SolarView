using SolarView.Common.Models;

namespace SolarView.Client.Common.Models
{
  public class EnergyCosts : IEnergyCosts
  {
    public double CostPerKWhPeak { get; set; }
    public double CostPerKWhOffPeak { get; set; }
    public double SolarBuyBack { get; set; }
  }
}