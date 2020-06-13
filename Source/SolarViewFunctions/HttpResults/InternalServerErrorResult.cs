using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace SolarViewFunctions.HttpResults
{
  public class InternalServerErrorResult : ObjectResult
  {
    public InternalServerErrorResult(Exception exception)
      : base(exception.Message)
    {
      StatusCode = (int)HttpStatusCode.InternalServerError;
    }
  }
}