using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SolarViewFunctions.Extensions
{
  public static class DurableOrchestrationContextExtensions
  {
    public static object GetTrackingProperties(this IDurableOrchestrationContext context)
    {
      return new
      {
        context.Name,
        context.ParentInstanceId,
        context.InstanceId,
        CurrentUtcDateTime = context.CurrentUtcDateTime.GetSolarDateTimeString()
      };
    }
  }
}