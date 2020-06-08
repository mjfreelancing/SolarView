namespace SolarViewFunctions.Dto
{
  public class HydratePowerRequest
  {
    public string SiteId { get; set; }
    public string StartDate { get; set; }     // must be yyyy-MM-dd
    public string EndDate { get; set; }
  }
}