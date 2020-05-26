using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SolarCosmosUtil.Models
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum MeterType
  {
    Production,             // produced by the panel/inverter
    Consumption,            // total consumption
    SelfConsumption,        // calculated (Production - FeedIn)
    FeedIn,                 // power fed back into the grid
    Purchased               // power taken from the grid
  }
}