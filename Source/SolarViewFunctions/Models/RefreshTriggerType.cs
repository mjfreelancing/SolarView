using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SolarViewFunctions.Models
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum RefreshTriggerType
  {
    Manual,
    Timed
  }
}