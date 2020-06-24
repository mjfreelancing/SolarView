using System.Collections.Generic;

namespace SolarViewFunctions.SolarEdge.Dto.Response
{
  public class MeterDto
  {
    public string Type { get; set; }
    public IEnumerable<MeterValueDto> Values { get; set; }
  }
}