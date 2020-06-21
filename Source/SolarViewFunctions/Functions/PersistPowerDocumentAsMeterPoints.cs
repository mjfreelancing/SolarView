using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Power;
using SolarViewFunctions.Tracking;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class PersistPowerDocumentAsMeterPoints : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public PersistPowerDocumentAsMeterPoints(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(PersistPowerDocumentAsMeterPoints))]
    public async Task Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var powerDocument = context.GetInput<PowerDocument>();

      var entities = powerDocument.Meters
        .SelectMany(
          meter => meter.Points,
          (meter, point) => new MeterPowerEntity(powerDocument.SiteId, point.Timestamp, meter.MeterType, point.Watts))
        .AsReadOnlyList();

      Tracker.TrackInfo($"Persisting {entities.Count} meter points from document {powerDocument.Id}");

      var powerRepository = _repositoryFactory.Create<IPowerRepository>(powerTable);
      await powerRepository.UpsertPowerAsync(entities).ConfigureAwait(false);
    }
  }
}