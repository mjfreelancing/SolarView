using AllOverIt.Helpers;
using SolarViewFunctions.SolarEdge.Dto.Response;

namespace SolarViewFunctions.Models
{
  public class SiteSolarData
  {
    public string SiteId { get; }
    public string StartDateTime { get; }   // must be yyyy-MM-dd HH:mm:ss
    public string EndDateTime { get; }
    public SolarData SolarData { get; }

    public SiteSolarData(string siteId, string startDateTime, string endDateTime, SolarData solarData)
    {
      SiteId = siteId.WhenNotNullOrEmpty(nameof(siteId));
      StartDateTime = startDateTime.WhenNotNullOrEmpty(nameof(startDateTime));
      EndDateTime = endDateTime.WhenNotNullOrEmpty(nameof(endDateTime));
      SolarData = solarData.WhenNotNull(nameof(solarData));
    }
  }
}