namespace SolarView.Client.Common.Models
{
  public class PowerData
  {
    public string Time { get; set; }
    public WattsData Watts { get; set; }
    public WattsData WattHour { get; set; }
  }
}
