using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Newtonsoft.Json;
using SolarViewFunctions.Models;
using System.Collections.Generic;

namespace SolarViewFunctions.Entities
{
  public class PowerDocument
  {
    [JsonProperty("id")]
    public string Id { get; set; }
    public string SiteId { get; set; }
    public string Date { get; set; }
    public IEnumerable<SolarViewMeter> Meters { get; set; }

    // required (even when upserting)
    public PowerDocument()
    {
    }

    public PowerDocument(string siteId, SolarViewDay solarDay)
    {
      SiteId = siteId.WhenNotNullOrEmpty(nameof(siteId));
      _ = solarDay.WhenNotNull(nameof(solarDay));

      Date = solarDay.Date;
      Meters = solarDay.Meters.AsReadOnlyList();

      Id = $"{SiteId}_{Date.Replace("-", string.Empty)}";
    }
  }
}