using AllOverIt.Helpers;
using System;
using System.Net;
using System.Runtime.Serialization;

namespace SolarViewFunctions.Exceptions
{
  [Serializable]
  public class SolarEdgeResponseException : Exception
  {
    public HttpStatusCode StatusCode { get; }
    public string SiteId { get; }
    public string StartDateTime { get; }
    public string EndDateTime { get; }

    public SolarEdgeResponseException()
    {
    }

    public SolarEdgeResponseException(string message)
      : base(message)
    {
    }

    public SolarEdgeResponseException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public SolarEdgeResponseException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public SolarEdgeResponseException(HttpStatusCode statusCode, string siteId, string startDateTime, string endDateTime)
      : this($"SolarEdge request failed with StatusCode {(int)statusCode} ({statusCode})")
    {
      StatusCode = statusCode;
      SiteId = siteId;
      StartDateTime = startDateTime;
      EndDateTime = endDateTime;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      _ = info.WhenNotNull(nameof(info));

      base.GetObjectData(info, context);

      info.AddValue(nameof(StatusCode), StatusCode);
      info.AddValue(nameof(SiteId), SiteId);
      info.AddValue(nameof(StartDateTime), StartDateTime);
      info.AddValue(nameof(EndDateTime), EndDateTime);
    }
  }
}