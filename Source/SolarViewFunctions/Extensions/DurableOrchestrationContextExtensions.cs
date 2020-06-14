using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Functions;
using System;
using System.Threading.Tasks;

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

    public static Task NotifyException<TSource>(this IDurableOrchestrationContext context, RetryOptions retryOptions, string siteId, Exception exception,
      object notification) where TSource : class
    {
      var exceptionDocument = new ExceptionDocument(typeof(TSource).Name, siteId, exception, notification);
      return context.CallActivityWithRetryAsync(nameof(ReportOrchestrationException), retryOptions, exceptionDocument);
    }
  }
}