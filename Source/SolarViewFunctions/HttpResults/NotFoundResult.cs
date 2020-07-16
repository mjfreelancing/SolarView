using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SolarViewFunctions.HttpResults
{
  public class NotFoundResult : ObjectResult
  {
    public NotFoundResult(object value)
      : base(value)
    {
      StatusCode = (int)HttpStatusCode.NotFound;
    }
  }
}