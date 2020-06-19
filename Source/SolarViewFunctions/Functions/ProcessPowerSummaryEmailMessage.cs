using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SendGrid.Helpers.Mail;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Providers;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.PowerUpdateHistory;
using SolarViewFunctions.Repository.Site;
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
    private readonly ISitesUpdateProvider _sitesUpdateProvider;
    private readonly ISendGridEmailCreator _emailCreator;

    public ProcessPowerSummaryEmailMessage(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory,
      ISitesUpdateProvider sitesUpdateProvider, ISendGridEmailCreator emailCreator)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
      _sitesUpdateProvider = sitesUpdateProvider.WhenNotNull(nameof(sitesUpdateProvider));
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

        Tracker.TrackEvent(nameof(ProcessPowerSummaryEmailMessage), request);

        var siteInfo = await _repositoryFactory.Create<ISiteRepository>(sitesTable).GetSiteAsync(request.SiteId);

        var updateHistoryRepository = _repositoryFactory.Create<IPowerUpdateHistoryRepository>(updateHistoryTable);

        var historyItems = await updateHistoryRepository.GetPowerUpdatesAsyncEnumerable(request.SiteId,
          request.StartDate.ParseSolarDate(), request.EndDate.ParseSolarDate());

        var historyGroups = historyItems
          .Where(item => item.Status != $"{PowerUpdatedStatus.Started}")
          .GroupBy(item => item.TriggerType);

        // temporary until razor can be used
        var content = new StringBuilder();
        content.AppendLine($"Power Update Summary for SiteId {request.SiteId} as of {request.EndDate}");
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
        await sendGridCollector.FlushAsync().ConfigureAwait(false);

        // update the sites table to indicate when the last summary email was sent
        await _sitesUpdateProvider.UpdateSiteAttributeAsync(sitesTable, siteInfo.SiteId, nameof(ISiteInfo.LastSummaryDate), request.EndDate);
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
          await exceptionDocuments.AddNotificationAsync<ProcessPowerSummaryEmailMessage>(request.SiteId, exception, notification).ConfigureAwait(false);
          await sendGridCollector.FlushAsync().ConfigureAwait(false);
        }
      }
    }
  }
}