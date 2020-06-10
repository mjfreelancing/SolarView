using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Tracking;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class GetSiteInfo : FunctionBase
  {
    public GetSiteInfo(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(GetSiteInfo))]
    public async Task<SiteInfo> Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Sites)] CloudTable sitesTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var siteId = context.GetInput<string>();

      Tracker.TrackInfo($"Getting info for SiteId {siteId}");

      return await sitesTable.GetItemAsync<SiteInfo>("SiteId", siteId).ConfigureAwait(false);
    }
  }
}