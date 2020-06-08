using Microsoft.Azure.Cosmos.Table;

namespace SolarViewFunctions.Entities
{
  public class PowerUpdateEntity : TableEntity
  {
    public string Trigger { get; set; }         // of type RefreshTriggerType
    public string Status { get; set; }          // of type PowerUpdatedStatus - Table doesn't support enums
    public string SiteId { get; set; }
    public string StartDate { get; set; }       // yyyy-MM-dd HH:mm:ss
    public string EndDate { get; set; }         // yyyy-MM-dd HH:mm:ss
  }
}