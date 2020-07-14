using SolarView.Client.Common.Models;

namespace SolarViewBlazor.Charts.Models
{
  // stores Watts or Watt-Hour data for a specific time (Consumption Chart)
  public class TimeWatts
  {
    public string Time { get; }
    public double Consumption { get; set; }
    public double Production { get; set; }
    public double FeedIn { get; set; }
    public double Purchased { get; set; }
    public double SelfConsumption { get; set; }

    public TimeWatts(string time, WattsData watts)
    {
      Time = time;
      Consumption = watts.Consumption;
      Production = watts.Production;
      FeedIn = watts.FeedIn;
      Purchased = watts.Purchased;
      SelfConsumption = watts.SelfConsumption;
    }
  }
}