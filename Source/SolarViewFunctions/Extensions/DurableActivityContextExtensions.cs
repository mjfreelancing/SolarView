using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SolarViewFunctions.Extensions
{
  public static class DurableActivityContextExtensions
  {
    public static object GetTrackingProperties(this IDurableActivityContext context)
    {
      return new
      {
        context.InstanceId
      };
    }
  }
}