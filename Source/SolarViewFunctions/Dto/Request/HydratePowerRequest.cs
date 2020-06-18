namespace SolarViewFunctions.Dto.Request
{
  public class HydratePowerRequest : SiteRequestBase
  {
    public string StartDate { get; set; }     // must be yyyy-MM-dd (local)
    public string EndDate { get; set; }
  }
}