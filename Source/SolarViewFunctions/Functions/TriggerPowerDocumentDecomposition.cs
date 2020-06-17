using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
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
        LeaseCollectionName = Constants.Cosmos.PowerLeases)] IReadOnlyList<Document> powerDocuments,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      Tracker.TrackEvent(nameof(TriggerPowerDocumentDecomposition));
      Tracker.TrackInfo($"Received {powerDocuments.Count} documents for decomposition");

      foreach (var document in powerDocuments)
      {
        try
        {
          var instanceId = await orchestrationClient.StartNewAsync(nameof(DecomposePowerDocument), document).ConfigureAwait(false);

          Tracker.TrackInfo($"Initiated {nameof(DecomposePowerDocument)} for document Id {document.Id}, InstanceId {instanceId}");
        }
        catch (Exception exception)
        {
          Tracker.TrackException(exception);

          PowerDocument powerDocument = (dynamic)document;

          await exceptionDocuments.AddNotificationAsync<TriggerPowerDocumentDecomposition>(powerDocument.SiteId, exception, null).ConfigureAwait(false);
        }
      }
    }
  }
}