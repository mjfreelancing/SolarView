using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarViewFunctions.Exceptions;

namespace SolarViewFunctions.HttpResults
{
  public class PreConditionErrorResult : ObjectResult
  {
    public PreConditionErrorResult(PreConditionException exception)
      : base(JsonConvert.SerializeObject(exception.Errors, Formatting.Indented))
    {
      StatusCode = (int)HttpStatusCode.PreconditionFailed;
    }
  }
}