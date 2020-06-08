using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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
      [ServiceBusTrigger(Constants.Queues.SolarPower, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] Message queueMessage,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      try
      {
        Tracker.TrackEvent(nameof(ProcessSiteRefreshPowerMessage));

        var refreshRequest = queueMessage.DeserializeFromMessage<SiteRefreshPowerRequest>();

        Tracker.TrackInfo($"Received a new {nameof(SiteRefreshPowerRequest)} message for SiteId {refreshRequest.SiteId}, DateTime {refreshRequest.DateTime}");

        var instanceId = await orchestrationClient.StartNewAsync(nameof(RefreshSitePowerDataOrchestrator), refreshRequest);

        Tracker.TrackInfo($"Initiated processing of the {nameof(SiteRefreshPowerRequest)} message for SiteId {refreshRequest.SiteId}, InstanceId = {instanceId}");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception, new { queueMessage.MessageId });

        // allow the message to be re-tried (or deadletter)
        throw;
      }
    }
  }
}