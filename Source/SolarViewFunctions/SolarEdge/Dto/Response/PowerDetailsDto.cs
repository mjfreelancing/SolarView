using System.Collections.Generic;

namespace SolarViewFunctions.SolarEdge.Dto.Response
{
  public class PowerDetailsDto
  {
    public IEnumerable<MeterDto> Meters { get; set; }
  }
}