using System.Net;
using System.Net.Http;

namespace SolarViewFunctions.HttpResults
{
  public class ForbiddenResponse : HttpResponseMessage
  {
    public ForbiddenResponse()
    {
      StatusCode = HttpStatusCode.Forbidden;
      ReasonPhrase = "Forbidden";
    }
  }
}