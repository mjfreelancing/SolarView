using Microsoft.Azure.Cosmos.Table;

namespace SolarViewFunctions.Entities
{
  // Sites table
  public class SiteInfo : TableEntity
  {
    // PartitionKey == "SiteId"
    // RowKey => value of SiteId
    public string SiteId { get; set; }
    public string StartDate { get; set; }                   // first full day where all meter data is available (yyyy-MM-dd)
    public string ContactName { get; set; }
    public string ContactEmail { get; set; }
    public string ApiKey { get; set; }
    public string TimeZoneId { get; set; }
    public string LastRefreshDateTime { get; set; }         // the last time a refresh was completed (yyyy-MM-dd HH:mm:ss)
    public string LastAggregationDate { get; set; }         // the last aggregation date (yyyy-MM-dd) - only ever performed up until midnight over the previous day
  }
}