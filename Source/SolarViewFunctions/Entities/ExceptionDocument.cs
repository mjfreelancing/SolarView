using Newtonsoft.Json;
using System;

namespace SolarViewFunctions.Entities
{
  public class ExceptionDocument
  {
    [JsonProperty("id")]
    public string Id { get; set; }
    public string SiteId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
    public string Exception { get; set; }
    public object Notification { get; set; }

    public ExceptionDocument()
    {
    }

    public ExceptionDocument(string source, string siteId, Exception exception, object notification)
    {
      Id = $"{Guid.NewGuid()}";
      SiteId = siteId;
      Timestamp = DateTime.UtcNow;
      Source = source;
      Exception = $"{exception}";
      Notification = notification;
    }
  }
}