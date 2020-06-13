using DurableTask.Core.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;

namespace SolarViewFunctions.Extensions
{
  public static class ExceptionExtensions
  {
    public static Exception UnwrapFunctionException(this Exception exception)
    {
      return exception switch
      {
        FunctionFailedException functionException => functionException.InnerException,
        SubOrchestrationFailedException subOrchestrationException => subOrchestrationException.InnerException,
        _ => exception
      };
    }
  }
}