using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Tracking;
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
    public Task Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      
      var siteInfo = context.GetInput<SiteInfo>();

      Tracker.TrackInfo($"Updating info for SiteId {siteInfo.SiteId}");

      var sitesRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);
      return sitesRepository.Upsert(siteInfo);
    }
  }
}