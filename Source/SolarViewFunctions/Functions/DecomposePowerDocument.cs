using AllOverIt.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class DecomposePowerDocument : FunctionBase
  {
    public DecomposePowerDocument(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(DecomposePowerDocument))]
    public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection, 
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      MakeTrackerReplaySafe(context);
      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      PowerDocument powerDocument = null;
      var document = context.GetInput<Document>();

      try
      {
        Tracker.TrackEvent(nameof(DecomposePowerDocument), new { DocumentId = document.Id });

        powerDocument = (dynamic)document;

        Tracker.TrackInfo($"Processing decomposition of document {document.Id}");

        await context.CallActivityAsync(nameof(PersistPowerDocumentAsMeterPoints), powerDocument);
      }
      catch (Exception exception)
      {
        var trackedException = exception.UnwrapFunctionException();

        var notification = new
        {
          powerDocument?.SiteId,
          DocumentId = document.Id,
          document.SelfLink,
          document.ResourceId
        };

        Tracker.TrackException(trackedException, notification);

        if (!powerDocument?.SiteId.IsNullOrEmpty() ?? false)
        {
          await exceptionDocuments.AddNotificationAsync<DecomposePowerDocument>(powerDocument.SiteId, trackedException, notification);
        }
      }
    }
  }
}
