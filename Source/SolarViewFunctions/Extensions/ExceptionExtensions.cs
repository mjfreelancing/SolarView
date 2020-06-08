using DurableTask.Core.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SolarViewFunctions.Exceptions;
using System;
using System.Net;
using System.Net.Http;

namespace SolarViewFunctions.Extensions
{
  public static class ExceptionExtensions
  {
    public static HttpResponseMessage GetInternalServerErrorResponse(this Exception exception)
    {
      return new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.InternalServerError,
        ReasonPhrase = "Internal Server Error",
        Content = new StringContent(exception.Message)
      };
    }

    public static HttpResponseMessage GetPreConditionErrorResponse(this PreConditionException exception)
    {
      return new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.PreconditionFailed,
        ReasonPhrase = "Precondition Failed",
        Content = new StringContent(JsonConvert.SerializeObject(exception.Errors, Formatting.Indented))
      };
    }

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