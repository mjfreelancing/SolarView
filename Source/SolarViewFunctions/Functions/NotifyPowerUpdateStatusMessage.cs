using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.ServiceBus;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models.Messages;
using SolarViewFunctions.Tracking;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class NotifyPowerUpdateStatusMessage : FunctionBase
  {
    public NotifyPowerUpdateStatusMessage(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(NotifyPowerUpdateStatusMessage))]
    public async Task<PowerUpdatedStatus> Run(
      [ActivityTrigger] IDurableActivityContext context,
      [ServiceBus(Constants.Queues.PowerUpdated, EntityType.Queue, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] MessageSender messagesQueue)
    {
      // allowing exceptions to bubble back to the caller

      var updatedMessage = context.GetInput<PowerUpdatedMessage>();

      Tracker.TrackEvent(nameof(NotifyPowerUpdateStatusMessage), new { context.InstanceId, updatedMessage.Status });

      var queueMessage = MessageHelpers.SerializeToMessage(updatedMessage, new { updatedMessage.Status });

      Tracker.TrackInfo($"Sending a {nameof(NotifyPowerUpdateStatusMessage)} for SiteId {updatedMessage.SiteId}, Status {updatedMessage.Status}");

      await messagesQueue.SendAsync(queueMessage).ConfigureAwait(false);

      return updatedMessage.Status;
    }
  }
}