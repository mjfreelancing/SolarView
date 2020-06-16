namespace SolarViewFunctions.Models
{
  public class SiteRefreshPowerRequest
  {
    public string SiteId { get; set; }
    public string StartDateTime { get; set; }      // assumed to be site local time
    public string EndDateTime { get; set; }
  }
}