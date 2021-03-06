namespace SolarViewFunctions.Models
{
  public class PowerUpdatedMessage
  {
    public string TriggerDateTime { get; set; }     // yyyy-MM-dd HH:mm:ss (local)
    public RefreshTriggerType TriggerType { get; set; }
    public PowerUpdatedStatus Status { get; set; }
    public string SiteId { get; set; }
    public string StartDateTime { get; set; }       // yyyy-MM-dd HH:mm:ss (local)
    public string EndDateTime { get; set; }         // yyyy-MM-dd HH:mm:ss (local)
  }
}