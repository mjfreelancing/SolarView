namespace SolarViewFunctions.Models
{
  public class SiteRefreshPowerRequest
  {
    public string SiteId { get; set; }
    public string DateTime { get; set; }      // assumed to be site local time
  }
}