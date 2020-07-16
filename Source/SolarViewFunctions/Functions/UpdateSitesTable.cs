using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarView.Common.Models;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Providers;
using SolarViewFunctions.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class UpdateSitesTable : FunctionBase
  {
    private readonly ISitesUpdateProvider _sitesUpdateProvider;

    public UpdateSitesTable(ITracker tracker, ISitesUpdateProvider sitesUpdateProvider)
      : base(tracker)
    {
      _sitesUpdateProvider = sitesUpdateProvider.WhenNotNull(nameof(sitesUpdateProvider));
    }

    [FunctionName(nameof(UpdateSitesTable))]
    public async Task Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var siteProperties = context.GetInput<Dictionary<string, object>>();

      var siteId = (string)siteProperties[nameof(ISiteDetails.SiteId)];

      Tracker.TrackInfo($"Updating info for SiteId {siteId}");

      var updateProperties = new Dictionary<string, object>();

      foreach (var (name, value) in siteProperties.Where(item => item.Key != nameof(ISiteDetails.SiteId)))
      {
        updateProperties.Add(name, value);
      }

      await _sitesUpdateProvider.UpdateSiteAttributeAsync(sitesTable, siteId, updateProperties).ConfigureAwait(false);
    }
  }
}