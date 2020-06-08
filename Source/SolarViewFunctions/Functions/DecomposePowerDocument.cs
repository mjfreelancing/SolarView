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
    public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      try
      {
        MakeTrackerReplaySafe(context);

        var document = context.GetInput<Document>();

        Tracker.TrackEvent(nameof(DecomposePowerDocument), new { DocumentId = document.Id });

        PowerDocument powerDocument = (dynamic)document;

        Tracker.TrackInfo($"Processing decomposition of document {document.Id}", new { context.InstanceId });

        await context.CallActivityAsync(nameof(PersistPowerDocumentAsMeterPoints), powerDocument);
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception.UnwrapFunctionException(), new { context.InstanceId });

        // todo: send a message to send an email
      }
    }
  }
}
