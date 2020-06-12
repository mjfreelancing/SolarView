using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.ServiceBus;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models.Messages;
using SolarViewFunctions.Tracking;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class NotifyPowerUpdatedMessage : FunctionBase
  {
    public NotifyPowerUpdatedMessage(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(NotifyPowerUpdatedMessage))]
    public async Task<PowerUpdatedStatus> Run(
      [ActivityTrigger] IDurableActivityContext context,
      [ServiceBus(Constants.Queues.PowerUpdated, EntityType.Queue, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] MessageSender messagesQueue)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.AppendDefaultProperties(context.GetTrackingProperties());
      
      var updatedMessage = context.GetInput<PowerUpdatedMessage>();

      Tracker.TrackEvent(nameof(NotifyPowerUpdatedMessage), updatedMessage);

      var queueMessage = MessageHelpers.SerializeToMessage(updatedMessage);

      Tracker.TrackInfo($"Sending a {nameof(NotifyPowerUpdatedMessage)} for SiteId {updatedMessage.SiteId}, Status {updatedMessage.Status}");

      await messagesQueue.SendAsync(queueMessage).ConfigureAwait(false);

      return updatedMessage.Status;
    }
  }
}