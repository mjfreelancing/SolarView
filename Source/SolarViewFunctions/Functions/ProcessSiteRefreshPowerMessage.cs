using AllOverIt.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class ProcessSiteRefreshPowerMessage : FunctionBase
  {
    public ProcessSiteRefreshPowerMessage(IRetryOptionsFactory retryOptionsFactory, ITracker tracker)
      : base(retryOptionsFactory, tracker)
    {
    }

    [FunctionName(nameof(ProcessSiteRefreshPowerMessage))]
    public async Task Run(
      [ServiceBusTrigger(Constants.Queues.PowerRefresh, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] Message queueMessage,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      Tracker.AppendDefaultProperties(new { queueMessage.MessageId });

      SiteRefreshPowerRequest request = null;

      try
      {
        Tracker.TrackEvent(nameof(ProcessSiteRefreshPowerMessage));

        request = queueMessage.DeserializeFromMessage<SiteRefreshPowerRequest>();

        Tracker.TrackInfo($"Received a new {nameof(SiteRefreshPowerRequest)} message for SiteId {request.SiteId}, ending {request.EndDateTime}");

        var instanceId = await orchestrationClient.StartNewAsync(nameof(RefreshSitePowerDataOrchestrator), request).ConfigureAwait(false);

        Tracker.TrackInfo(
          $"Initiated processing of the {nameof(SiteRefreshPowerRequest)} message for SiteId {request.SiteId}",
          new {InstanceId = instanceId}
        );
      }
      catch (Exception exception)
      {
        var notification = new
        {
          request?.SiteId,
          QueueMessageId = queueMessage.MessageId,
          MessageContent = request
        };

        Tracker.TrackException(exception, notification);

        if (!request?.SiteId.IsNullOrEmpty() ?? false)
        {
          await exceptionDocuments.AddNotificationAsync<ProcessSiteRefreshPowerMessage>(request.SiteId, exception, notification).ConfigureAwait(false);
          await exceptionDocuments.FlushAsync().ConfigureAwait(false);
        }
      }
    }
  }
}