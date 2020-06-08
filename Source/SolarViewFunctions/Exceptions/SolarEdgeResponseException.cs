using System;
using System.Net;

namespace SolarViewFunctions.Exceptions
{
  public class SolarEdgeResponseException : Exception
  {
    public HttpStatusCode StatusCode { get; }
    public string SiteId { get; }
    public string StartDateTime { get; }
    public string EndDateTime { get; }

    public SolarEdgeResponseException(HttpStatusCode statusCode, string siteId, string startDateTime, string endDateTime)
      : base($"SolarEdge request failed with StatusCode {(int)statusCode} ({statusCode})")
    {
      StatusCode = statusCode;
      SiteId = siteId;
      StartDateTime = startDateTime;
      EndDateTime = endDateTime;
    }
  }
}