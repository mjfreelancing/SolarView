using AllOverIt.Extensions;
using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
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
      [Table(Constants.Table.PowerUpdateHistory, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable historyTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      Tracker.AppendDefaultProperties(new { queueMessage.MessageId });

      PowerUpdatedMessage request = null;

      try
      {
        request = queueMessage.DeserializeFromMessage<PowerUpdatedMessage>();

        Tracker.TrackEvent(nameof(ProcessPowerUpdatedHistoryMessage), new { queueMessage.MessageId, request.Status });

        Tracker.TrackInfo($"Updating {nameof(Constants.Table.PowerUpdateHistory)} table for SiteId {request.SiteId} with status " +
                          $"{request.Status} for date range {request.StartDateTime} to {request.EndDateTime}");

        var entity = _mapper.Map<PowerUpdate>(request);

        var historyRepository = _repositoryFactory.Create<IPowerUpdateHistoryRepository>(historyTable);
        await historyRepository.UpsertAsync(entity).ConfigureAwait(false);
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
          await exceptionDocuments.AddNotificationAsync<ProcessPowerUpdatedHistoryMessage>(request.SiteId, exception, notification).ConfigureAwait(false);
          await exceptionDocuments.FlushAsync().ConfigureAwait(false);
        }
      }
    }
  }
}
