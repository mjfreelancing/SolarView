using System.Collections.Generic;

namespace SolarViewFunctions.Models.SolarEdgeData
{
  public class Meter
  {
    public string Type { get; set; }
    public IEnumerable<MeterValue> Values { get; set; }
  }
}