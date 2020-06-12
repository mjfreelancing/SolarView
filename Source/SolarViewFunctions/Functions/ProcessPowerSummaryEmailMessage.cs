using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SendGrid.Helpers.Mail;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Models.Messages;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.PowerUpdateHistory;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.SendGrid;
using SolarViewFunctions.Tracking;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class ProcessPowerSummaryEmailMessage : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;
    private readonly ISendGridEmailCreator _emailCreator;

    public ProcessPowerSummaryEmailMessage(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory, ISendGridEmailCreator emailCreator)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
      _emailCreator = emailCreator.WhenNotNull(nameof(emailCreator));
    }

    [FunctionName(nameof(ProcessPowerSummaryEmailMessage))]
    public async Task Run(
      [ServiceBusTrigger(Constants.Queues.SummaryEmail, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] Message queueMessage,
      //MessageReceiver messageReceiver, string lockToken,        // todo: can use this to deadletter a message
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [Table(Constants.Table.PowerUpdateHistory, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable updateHistoryTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [SendGrid(ApiKey = "SolarViewSendGridApiKey")] IAsyncCollector<SendGridMessage> sendGridCollector)
    {
      Tracker.AppendDefaultProperties(new { queueMessage.MessageId });

      SiteSummaryEmailRequest request = null;

      try
      {
        request = queueMessage.DeserializeFromMessage<SiteSummaryEmailRequest>();

        // the date will be for the previous day
        Tracker.TrackEvent(nameof(ProcessPowerSummaryEmailMessage), new { request.SiteId, Date = request.LocalDate });

        var siteInfo = await _repositoryFactory.Create<ISitesRepository>(sitesTable).GetSiteAsync(request.SiteId);

        var updateHistoryRepository = _repositoryFactory.Create<IPowerUpdateHistoryRepository>(updateHistoryTable);
        var historyItems = await updateHistoryRepository.GetPowerUpdatesAsyncEnumerable(request.SiteId, request.LocalDate.ParseSolarDate());

        var historyGroups = historyItems
          .Where(item => item.Status != $"{PowerUpdatedStatus.Started}")
          .GroupBy(item => item.Trigger);

        // temporary until razor can be used
        var content = new StringBuilder();
        content.AppendLine($"Power Update Summary for SiteId {request.SiteId} on {request.LocalDate}");
        content.AppendLine();

        foreach (var kvp in historyGroups)
        {
          var trigger = kvp.Key;
          var items = kvp.OrderBy(item => item.TriggerDateTime).AsReadOnlyList();

          content.AppendLine($"Trigger: {trigger}");

          foreach (var item in items)
          {
            var line = $"(Triggered {item.TriggerDateTime}) for {item.StartDateTime} - {item.EndDateTime}, Status = {item.Status}";
            content.AppendLine(line);
          }

          content.AppendLine();
        }

        Tracker.TrackInfo($"Sending summary email for SiteId {siteInfo.SiteId}");

        //text/html
        var email = _emailCreator.CreateMessage(siteInfo, "Power Update Summary", "text/plain", $"{content}");
        await sendGridCollector.AddAsync(email).ConfigureAwait(false);
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
          await exceptionDocuments.AddNotificationAsync<ProcessPowerSummaryEmailMessage>(request.SiteId, exception, notification);
        }
      }
    }
  }
}