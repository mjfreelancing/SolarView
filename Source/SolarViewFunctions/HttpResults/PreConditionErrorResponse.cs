using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using SolarViewFunctions.Exceptions;

namespace SolarViewFunctions.HttpResults
{
  public class PreConditionErrorResponse : HttpResponseMessage
  {
    public PreConditionErrorResponse(PreConditionException exception)
    {
      StatusCode = HttpStatusCode.PreconditionFailed;
      ReasonPhrase = "Precondition Failed";
      Content = new StringContent(JsonConvert.SerializeObject(exception.Errors, Formatting.Indented));
    }
  }
}