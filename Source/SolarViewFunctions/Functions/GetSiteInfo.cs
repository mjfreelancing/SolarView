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
  public class GetSiteInfo : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public GetSiteInfo(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(GetSiteInfo))]
    public Task<SiteInfo> Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var siteId = context.GetInput<string>();

      Tracker.TrackInfo($"Getting info for SiteId {siteId}");

      return _repositoryFactory.Create<ISitesRepository>(sitesTable).GetSiteAsync(siteId);
    }
  }
}