using System.Threading.Tasks;

namespace SolarViewBlazor.Models
{
  public interface IAppState
  {
    public string SiteId { get; }
    public string StartDate { get; }
    public string ContactName { get; }
    public string ContactEmail { get; }
    public string TimeZoneId { get; }
    public string LastRefreshDateTime { get; }
    public string LastAggregationDate { get; }
    public string LastSummaryDate { get; }

    Task<bool> LoadLastKnownSite();
    Task<bool> SetSiteAsync(string siteId);
  }
}