namespace SolarView.Common.Models
{
  public interface ISiteInfo
  {
    string SiteId { get; }
    string StartDate { get; }                   // first full day where all meter data is available (yyyy-MM-dd)
    string ContactName { get; }
    string ContactEmail { get; }
    string TimeZoneId { get; }
    string LastRefreshDateTime { get; }         // the last time a refresh was completed (yyyy-MM-dd HH:mm:ss)
    string LastAggregationDate { get; }         // the last aggregation date (yyyy-MM-dd) - only ever performed up until midnight over the previous day
    string LastSummaryDate { get; }             // the last time a summary email was sent (yyyy-MM-dd)
  }
}