using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

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