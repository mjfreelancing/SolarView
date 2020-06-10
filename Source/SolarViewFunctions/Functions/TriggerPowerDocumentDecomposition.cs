using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerPowerDocumentDecomposition : FunctionBase
  {
    public TriggerPowerDocumentDecomposition(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(TriggerPowerDocumentDecomposition))]
    public async Task Run(
      [CosmosDBTrigger(Constants.Cosmos.SolarDatabase, Constants.Cosmos.PowerCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos,
        LeaseCollectionName = Constants.Cosmos.PowerLeases)] IReadOnlyList<Document> documents,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      try
      {
        Tracker.TrackEvent(nameof(TriggerPowerDocumentDecomposition),
          new
          {
            TriggerTimeUtc = $"{DateTime.UtcNow.GetSolarDateTimeString()} (UTC)",
            DocumentCount = documents.Count
          });

        Tracker.TrackInfo($"Received {documents.Count} documents for decomposition");

        var tasks = documents.Select(document => orchestrationClient.StartNewAsync(nameof(DecomposePowerDocument), document));

        var instanceIds = await Task.WhenAll(tasks);

        Tracker.TrackInfo(
          $"Started {documents.Count} instance(s) of {nameof(DecomposePowerDocument)}",
          new { InstanceIds = string.Join(", ", instanceIds) }
        );
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);

        // todo: send a message to send an email
      }
    }
  }
}