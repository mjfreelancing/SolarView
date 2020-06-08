using Microsoft.Azure.Cosmos.Table;

namespace SolarViewFunctions.Entities
{
  // Sites table
  public class SiteInfo : TableEntity
  {
    // PartitionKey == "SiteId"
    // RowKey => value of SiteId
    public string SiteId { get; set; }
    public string InstallDate { get; set; }
    public string ContactName { get; set; }
    public string ContactEmail { get; set; }
    public string ApiKey { get; set; }
    public string TimeZoneId { get; set; }
    public string LastRefreshEnd { get; set; }
    public string NextRefreshDue { get; set; }
  }
}