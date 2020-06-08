using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models.Messages;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class ProcessPowerUpdateHistoryMessage : FunctionBase
  {
    private readonly IMapper _mapper;

    public ProcessPowerUpdateHistoryMessage(IMapper mapper, ITracker tracker)
      : base(tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
    }

    [FunctionName(nameof(ProcessPowerUpdateHistoryMessage))]
    public async Task Run(
      [ServiceBusTrigger(Constants.Queues.PowerUpdated, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] Message queueMessage,
      [Table(Constants.Table.PowerUpdateHistory)] CloudTable historyTable)
    {
      try
      {
        var updatedMessage = queueMessage.DeserializeFromMessage<PowerUpdatedMessage>();

        Tracker.TrackEvent(nameof(ProcessPowerUpdateHistoryMessage), new { queueMessage.MessageId, updatedMessage.Status });

        Tracker.TrackInfo($"Updating {nameof(Constants.Table.PowerUpdateHistory)} table for SiteId {updatedMessage.SiteId} with status " +
                          $"{updatedMessage.Status} for date range {updatedMessage.StartDate} to {updatedMessage.EndDate}");

        var entity = _mapper.Map<PowerUpdateEntity>(updatedMessage);
        await historyTable.InsertOrReplaceAsync(entity).ConfigureAwait(false);
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
