using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SolarViewFunctions.Models.Messages
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum PowerUpdatedStatus
  {
    Started,
    Completed,
    Error
  }
}