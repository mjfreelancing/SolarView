using System.Collections.Generic;

namespace SolarCosmosUtil.Dto.Response
{
  public class PowerDetails
  {
    //QUARTER_OF_AN_HOUR
    //public string TimeUnit { get; set; }

    //W
    //public string Unit { get; set; }

    public IEnumerable<Meter> Meters { get; set; }
  }
}