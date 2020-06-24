using System.Collections.Generic;

namespace SolarViewFunctions.Models
{
  public class SolarViewDay
  {
    public string SiteId { get; set; }
    public string Date { get; set; }
    public IEnumerable<SolarViewMeter> Meters { get; set; }
  }
}