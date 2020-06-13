namespace SolarViewFunctions.Models
{
  public class TimeWatts
  {
    public string Time { get; }
    public double Watts { get; }

    public TimeWatts(string time, double watts)
    {
      Time = time;
      Watts = watts;
    }
  }
}