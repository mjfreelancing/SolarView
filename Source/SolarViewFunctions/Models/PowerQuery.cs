namespace SolarViewFunctions.Models
{
  public class PowerQuery
  {
    public string SiteId { get; set; }
    public string StartDateTime { get; set; }   // local time, must be yyyy-MM-dd HH:mm:ss
    public string EndDateTime { get; set; }
  }
}