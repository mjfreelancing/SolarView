namespace SolarViewFunctions.Models
{
  public class TimeWatts
  {
    public string Time { get; }
    public double Watts { get; }
    public double WattHour { get; }

    public TimeWatts(string time, double watts, double wattHour)
    {
      Time = time;
      Watts = watts;
      WattHour = wattHour;
    }
  }
}