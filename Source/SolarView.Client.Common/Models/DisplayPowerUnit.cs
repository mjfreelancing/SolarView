namespace SolarView.Client.Common.Models
{
  public class DisplayPowerUnit
  {
    public PowerUnit Id { get; }
    public string Text { get; }

    public DisplayPowerUnit(PowerUnit id, string text)
    {
      Id = id;
      Text = text;
    }
  }
}