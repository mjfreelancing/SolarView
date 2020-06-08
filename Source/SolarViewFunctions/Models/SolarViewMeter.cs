using System.Collections.Generic;

namespace SolarViewFunctions.Models
{
  public class SolarViewMeter
  {
    public MeterType MeterType { get; set; }
    public IEnumerable<SolarViewMeterPoint> Points { get; set; }
  }
}