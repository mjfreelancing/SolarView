namespace SolarViewFunctions.Models
{
  public class SiteRefreshAggregationRequest
  {
    public string SiteId { get; set; }
    public string StartDate { get; set; }     // assumed to be site local date (no time)
    public string EndDate { get; set; }
  }
}