using SolarViewFunctions.Extensions;
using System;

namespace SolarViewFunctions.Models
{
  public class TriggeredPowerQuery : PowerQuery
  {
    public string TriggerDateTime { get; set; }         // yyyy-MM-dd HH:mm:ss (local)
    public RefreshTriggerType TriggerType { get; set; }

    public TriggeredPowerQuery()
    {
    }

    public TriggeredPowerQuery(DateTime triggerDateTime, RefreshTriggerType triggerType)
    {
      TriggerDateTime = triggerDateTime.GetSolarDateTimeString();
      TriggerType = triggerType;
    }
  }
}