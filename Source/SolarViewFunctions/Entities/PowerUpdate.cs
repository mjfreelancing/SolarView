using Microsoft.Azure.Cosmos.Table;

namespace SolarViewFunctions.Entities
{
  public class PowerUpdate : TableEntity
  {
    public string TriggerDateTime { get; set; }     // yyyy-MM-dd HH:mm:ss (local)
    public string Trigger { get; set; }             // of type RefreshTriggerType
    public string Status { get; set; }              // of type PowerUpdatedStatus - Table doesn't support enums
    public string SiteId { get; set; }
    public string StartDateTime { get; set; }       // yyyy-MM-dd HH:mm:ss (local)
    public string EndDateTime { get; set; }         // yyyy-MM-dd HH:mm:ss (local)
  }
}