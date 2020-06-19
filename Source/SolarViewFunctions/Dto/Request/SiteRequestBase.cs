namespace SolarViewFunctions.Dto.Request
{
  public abstract class SitePeriodRequestBase
  {
    public string SiteId { get; set; }
    public string StartDate { get; set; }     // must be yyyy-MM-dd (local)
    public string EndDate { get; set; }
  }
}