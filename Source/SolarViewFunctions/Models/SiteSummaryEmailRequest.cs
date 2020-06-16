namespace SolarViewFunctions.Models
{
  public class SiteSummaryEmailRequest
  {
    public string SiteId { get; set; }
    public string StartDate { get; set; }     // local date yyyy-MM-dd
    public string EndDate { get; set; }
  }
}