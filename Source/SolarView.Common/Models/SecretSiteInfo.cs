namespace SolarView.Common.Models
{
  public class SecretSiteInfo : ISiteInfo, ISecretSiteInfo
  {
    public string SiteId { get; set; }
    public string StartDate { get; set; }
    public string ContactName { get; set; }
    public string ContactEmail { get; set; }
    public string ApiKey { get; set; }
    public string TimeZoneId { get; set; }
    public string LastRefreshDateTime { get; set; }
    public string LastAggregationDate { get; set; }
    public string LastSummaryDate { get; set; }
  }
}