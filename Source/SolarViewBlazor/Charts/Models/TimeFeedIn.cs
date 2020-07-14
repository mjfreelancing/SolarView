namespace SolarViewBlazor.Charts.Models
{
  // stores FeedIn value for a specific time (FeedIn chart)
  public class TimeFeedIn
  {
    public string Time { get; }
    public double FeedIn { get; }

    public TimeFeedIn(string time, double feedIn)
    {
      Time = time;
      FeedIn = feedIn;
    }
  }
}