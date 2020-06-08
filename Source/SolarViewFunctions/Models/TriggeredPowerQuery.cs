namespace SolarViewFunctions.Models
{
  public class TriggeredPowerQuery : PowerQuery
  {
    public RefreshTriggerType Trigger { get; set; }    // of type RefreshTriggerType
  }
}