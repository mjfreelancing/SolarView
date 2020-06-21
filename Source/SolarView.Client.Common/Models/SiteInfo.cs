using SolarView.Common.Models;

namespace SolarView.Client.Common.Models
{
  public class SiteInfo : ISiteInfo
  {
    public string SiteId { get; set; }
    public string StartDate { get; set; }
    public string ContactName { get; set; }
    public string ContactEmail { get; set; }
    public string TimeZoneId { get; set; }
    public string LastRefreshDateTime { get; set; }
    public string LastAggregationDate { get; set; }
    public string LastSummaryDate { get; set; }
  }
}