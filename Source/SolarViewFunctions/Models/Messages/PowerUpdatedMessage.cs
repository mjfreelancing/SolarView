namespace SolarViewFunctions.Models.Messages
{
  public class PowerUpdatedMessage
  {
    public RefreshTriggerType Trigger { get; set; }
    public PowerUpdatedStatus Status { get; set; }
    public string SiteId { get; set; }
    public string StartDate { get; set; }           // yyyy-MM-dd HH:mm:ss
    public string EndDate { get; set; }             // yyyy-MM-dd HH:mm:ss
  }
}