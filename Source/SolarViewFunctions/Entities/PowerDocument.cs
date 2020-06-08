using AllOverIt.Extensions;
using AllOverIt.Helpers;
using SolarViewFunctions.Models;
using System;
using System.Collections.Generic;

namespace SolarViewFunctions.Entities
{
  // energy container of the solar database
  public class PowerDocument
  {
    public string id => $"{Site}_{Date:yyyyMMdd}";
    public string Site { get; set; }
    public DateTime Date { get; set; }
    public IEnumerable<SolarViewMeter> Meters { get; set; }

    // required by cosmos (even when upserting)
    public PowerDocument()
    {
    }

    public PowerDocument(string siteId, SolarViewDay solarDay)
    {
      Site = siteId.WhenNotNullOrEmpty(nameof(siteId));
      _ = solarDay.WhenNotNull(nameof(solarDay));

      Date = solarDay.Date;
      Meters = solarDay.Meters.AsReadOnlyList();
    }
  }
}