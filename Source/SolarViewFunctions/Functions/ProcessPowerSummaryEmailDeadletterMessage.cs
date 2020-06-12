using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.SendGrid;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class ProcessPowerSummaryEmailDeadletterMessage : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    private readonly ISendGridEmailCreator _emailCreator;

    public ProcessPowerSummaryEmailDeadletterMessage(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory, ISendGridEmailCreator emailCreator)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
      _emailCreator = emailCreator.WhenNotNull(nameof(emailCreator));
    }

    [FunctionName(nameof(ProcessPowerSummaryEmailDeadletterMessage))]
    public async Task Run(
      [ServiceBusTrigger(Constants.Queues.SummaryEmailDeadletter, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] Message queueMessage,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [SendGrid(ApiKey = "SolarViewSendGridApiKey")] IAsyncCollector<SendGridMessage> sendGridCollector)
    {
      Tracker.AppendDefaultProperties(new { queueMessage.MessageId });

      SiteSummaryEmailRequest request = null;

      try
      {
        Tracker.TrackEvent(nameof(ProcessPowerSummaryEmailDeadletterMessage));

        request = queueMessage.DeserializeFromMessage<SiteSummaryEmailRequest>();

        var siteInfo = await _repositoryFactory.Create<ISitesRepository>(sitesTable).GetSiteAsync(request.SiteId);

        // todo: update to load a specific razor template - just send the model in the email for now
        var content = JsonConvert.SerializeObject(request);

        var email = _emailCreator.CreateMessage(siteInfo, "Power Update Summary - Deadletter", "text/plain", content);
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
          await exceptionDocuments.AddNotificationAsync<NotifyPowerUpdatedDeadletterMessage>(request.SiteId, exception, notification);
        }
      }
    }
  }
}