﻿using System.Collections.Generic;

namespace SolarViewFunctions.SolarEdge.Dto.Response
{
  public class Meter
  {
    public string Type { get; set; }
    public IEnumerable<MeterValue> Values { get; set; }
  }
}