using AllOverIt.Extensions;
using AllOverIt.Helpers;
using SolarCosmosUtil.Models;
using System;
using System.Collections.Generic;

namespace SolarCosmosUtil.Entities
{
  public class SolarDocument
  {
    public string id => $"{Site}_{Date:yyyyMMdd}";
    public string Site { get; set; }
    public DateTime Date { get; set; }
    public IEnumerable<SolarViewMeter> Meters { get; set; }

    // required by cosmos (even when upserting)
    public SolarDocument()
    {
    }

    public SolarDocument(string siteId, SolarViewDay solarDay)
    {
      Site = siteId.WhenNotNullOrEmpty(nameof(siteId));
      _ = solarDay.WhenNotNull(nameof(solarDay));

      Date = solarDay.Date;
      Meters = solarDay.Meters.AsReadOnlyList();
    }
  }
}