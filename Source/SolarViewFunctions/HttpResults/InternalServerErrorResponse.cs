using System;
using System.Net;
using System.Net.Http;

namespace SolarViewFunctions.HttpResults
{
  public class InternalServerErrorResponse : HttpResponseMessage
  {
    public InternalServerErrorResponse(Exception exception)
    {
      StatusCode = HttpStatusCode.InternalServerError;
      ReasonPhrase = "Internal Server Error";
      Content = new StringContent(exception.Message);
    }
  }
}