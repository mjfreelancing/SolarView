using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Tracking;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class ReportOrchestrationException : FunctionBase
  {
    public ReportOrchestrationException(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(ReportOrchestrationException))]
    public async Task Run([ActivityTrigger] IDurableActivityContext context,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      Tracker.TrackEvent(nameof(ReportOrchestrationException));

      var exceptionDocument = context.GetInput<ExceptionDocument>();

      await exceptionDocuments.AddAsync(exceptionDocument).ConfigureAwait(false);
      await exceptionDocuments.FlushAsync().ConfigureAwait(false);
    }
  }
}