namespace SolarViewFunctions.Models
{
  public class SiteRefreshAggregationRequest
  {
    public string SiteId { get; set; }
    public string SiteStartDate { get; set; }
    public string StartDate { get; set; }             // local date (no time)
    public string EndDate { get; set; }
    public RefreshTriggerType TriggerType { get; set; }
  }
}