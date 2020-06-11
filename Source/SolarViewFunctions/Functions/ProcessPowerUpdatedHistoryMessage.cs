using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models.Messages;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.PowerUpdateHistory;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class ProcessPowerUpdatedHistoryMessage : FunctionBase
  {
    private readonly IMapper _mapper;
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public ProcessPowerUpdatedHistoryMessage(IMapper mapper, ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(ProcessPowerUpdatedHistoryMessage))]
    public async Task Run(
      [ServiceBusTrigger(Constants.Queues.PowerUpdated, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] Message queueMessage,
      [Table(Constants.Table.PowerUpdateHistory, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable historyTable)
    {
      try
      {
        Tracker.AppendDefaultProperties(new { queueMessage.MessageId });

        var updatedMessage = queueMessage.DeserializeFromMessage<PowerUpdatedMessage>();

        Tracker.TrackEvent(nameof(ProcessPowerUpdatedHistoryMessage), new { queueMessage.MessageId, updatedMessage.Status });

        Tracker.TrackInfo($"Updating {nameof(Constants.Table.PowerUpdateHistory)} table for SiteId {updatedMessage.SiteId} with status " +
                          $"{updatedMessage.Status} for date range {updatedMessage.StartDateTime} to {updatedMessage.EndDateTime}");

        var entity = _mapper.Map<PowerUpdate>(updatedMessage);

        var historyRepository = _repositoryFactory.Create<IPowerUpdateHistoryRepository>(historyTable);
        await historyRepository.Upsert(entity).ConfigureAwait(false);
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);

        // allow the message to be re-tried (or deadletter)
        throw;
      }
    }
  }
}
