using System.Net;
using System.Net.Http;

namespace SolarViewFunctions.HttpResults
{
  public class NotFoundResponse : HttpResponseMessage
  {
    public NotFoundResponse()
    {
      StatusCode = HttpStatusCode.NotFound;
      ReasonPhrase = "Not Found";
    }
  }
}