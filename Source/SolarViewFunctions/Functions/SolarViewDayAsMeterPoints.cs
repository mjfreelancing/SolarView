using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Power;
using SolarViewFunctions.Tracking;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class SolarViewDayAsMeterPoints : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public SolarViewDayAsMeterPoints(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(SolarViewDayAsMeterPoints))]
    public async Task Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerTable)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var solarViewDay = context.GetInput<SolarViewDay>();

      var entities = solarViewDay.Meters
        .SelectMany(
          meter => meter.Points,
          (meter, point) => new MeterPowerEntity(solarViewDay.SiteId, point.Timestamp, meter.MeterType, point.Watts))
        .AsReadOnlyList();

      Tracker.TrackInfo($"Persisting {entities.Count} meter points for {solarViewDay.Date}");

      var powerRepository = _repositoryFactory.Create<IPowerRepository>(powerTable);
      await powerRepository.UpsertPowerAsync(entities).ConfigureAwait(false);
    }
  }
}