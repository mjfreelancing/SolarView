using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class UpdateSitesTable : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public UpdateSitesTable(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(UpdateSitesTable))]
    public async Task Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var siteProperties = context.GetInput<Dictionary<string, string>>();

      var siteId = siteProperties[Constants.Table.SitesPartitionKey];

      var entity = new DynamicTableEntity(Constants.Table.SitesPartitionKey, siteId)
      {
        // will be updating SiteInfo.LastAggregationDate or SiteInfo.LastRefreshDateTime
        Properties = siteProperties.ToDictionary(kvp =>kvp.Key,kvp=> new EntityProperty(kvp.Value))
      };

      Tracker.TrackInfo($"Updating info for SiteId {siteId}");

      var sitesRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);
      await sitesRepository.MergeAsync(entity);
    }
  }
}