using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SolarViewFunctions.HttpResults
{
  public class ForbiddenResult : ObjectResult
  {
    public ForbiddenResult(object value)
      : base(value)
    {
      StatusCode = (int)HttpStatusCode.Forbidden;
    }
  }
}