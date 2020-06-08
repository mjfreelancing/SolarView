using System;
using System.Collections.Generic;

namespace SolarViewFunctions.Models
{
  public class SolarViewDay
  {
    public DateTime Date { get; set; }
    public IEnumerable<SolarViewMeter> Meters { get; set; }
  }
}