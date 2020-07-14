using System.Collections.Generic;

namespace SolarViewFunctions.SolarEdge.Dto.Response
{
  public class EnergyDetailsDto
  {
    public IEnumerable<MeterDto> Meters { get; set; }
  }
}