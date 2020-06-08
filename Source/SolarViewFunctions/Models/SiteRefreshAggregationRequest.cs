namespace SolarViewFunctions.Models
{
  public class SiteRefreshAggregationRequest
  {
    public string SiteId { get; set; }
    public string DateTime { get; set; }      // assumed to be site local time
  }
}