using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Tracking;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class UpdateSitesTable : FunctionBase
  {
    public UpdateSitesTable(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(UpdateSitesTable))]
    public async Task Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Sites)] CloudTable sitesTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      
      var siteInfo = context.GetInput<SiteInfo>();

      Tracker.TrackInfo($"Updating info for SiteId {siteInfo.SiteId}");

      await sitesTable.InsertOrReplaceAsync(siteInfo).ConfigureAwait(false);
    }
  }
}