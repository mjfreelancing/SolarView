using System.Collections.Generic;

namespace SolarViewFunctions.SolarEdge.Dto.Response
{
  public class PowerDetails
  {
    public IEnumerable<Meter> Meters { get; set; }
  }
}