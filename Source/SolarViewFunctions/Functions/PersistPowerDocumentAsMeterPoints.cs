using AllOverIt.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Tracking;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class PersistPowerDocumentAsMeterPoints : FunctionBase
  {
    public PersistPowerDocumentAsMeterPoints(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(PersistPowerDocumentAsMeterPoints))]
    public async Task Run(
      [ActivityTrigger] IDurableActivityContext context,
      [Table(Constants.Table.Power)] CloudTable powerTable)
    {
      // allowing exceptions to bubble back to the caller

      var powerDocument = context.GetInput<PowerDocument>();

      var entities = powerDocument.Meters
        .SelectMany(
          meter => meter.Points,
          (meter, point) => new MeterPower(powerDocument.Site, point.Timestamp, meter.MeterType, point.Watts))
        .AsReadOnlyList();

      Tracker.TrackInfo($"Persisting {entities.Count} meter points from document {powerDocument.id}");

      await powerTable.BatchInsertOrMergeAsync(entities).ConfigureAwait(false);
    }
  }
}