using System.Collections.Generic;

namespace SolarCosmosUtil.Models
{
  public class SolarViewMeter
  {
    public MeterType MeterType { get; set; }
    public IEnumerable<SolarViewMeterPoint> Points { get; set; }
  }
}