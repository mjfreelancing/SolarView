using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SendGrid.Helpers.Mail;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Models.Messages;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.PowerUpdateHistory;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Tracking;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  // todo: write another function to process deadletter messages

  public class ProcessPowerSummaryEmailMessage : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public ProcessPowerSummaryEmailMessage(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(ProcessPowerSummaryEmailMessage))]
    public async Task Run(
      [ServiceBusTrigger(Constants.Queues.SummaryEmail, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] Message queueMessage,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [Table(Constants.Table.PowerUpdateHistory, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable updateHistoryTable,
      [SendGrid(ApiKey = "SolarViewSendGridApiKey")] IAsyncCollector<SendGridMessage> sendGridCollector)
    {
      try
      {
        Tracker.AppendDefaultProperties(new { queueMessage.MessageId });

        var emailRequest = queueMessage.DeserializeFromMessage<SiteSummaryEmailRequest>();

        // the date will be for the previous day
        Tracker.TrackEvent(nameof(ProcessPowerSummaryEmailMessage), new { emailRequest.SiteId, Date = emailRequest.LocalDate });

        var sitesRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);
        var updateHistoryRepository = _repositoryFactory.Create<IPowerUpdateHistoryRepository>(updateHistoryTable);

        var siteInfo = await sitesRepository.GetSiteAsync(emailRequest.SiteId);

        var historyItems = await updateHistoryRepository.GetPowerUpdatesAsyncEnumerable(emailRequest.SiteId, emailRequest.LocalDate.ParseSolarDate());

        var historyGroups = historyItems
          .Where(item => item.Status != $"{PowerUpdatedStatus.Started}")
          .GroupBy(item => item.Trigger);

        // temporary until markdown is used
        var content = new StringBuilder();
        content.AppendLine($"Power Update Summary for SiteId {emailRequest.SiteId} on {emailRequest.LocalDate}");
        content.AppendLine();

        foreach (var kvp in historyGroups)
        {
          var trigger = kvp.Key;
          var items = kvp.AsReadOnlyList();

          content.AppendLine($"Trigger: {trigger}");

          foreach (var item in items)
          {
            var line = $"{item.StartDateTime} - {item.EndDateTime} : {item.Status}";
            content.AppendLine(line);
          }

          content.AppendLine();
        }

        var email = new SendGridMessage();
        email.AddTo(siteInfo.ContactEmail);
        //email.AddContent("text/html", $"{content}");
        email.AddContent("text/plain", $"{content}");
        email.SetFrom(new EmailAddress(siteInfo.ContactEmail, siteInfo.ContactName));
        email.SetSubject("Power Update Summary");

        Tracker.TrackInfo($"Sending summary email for SiteId {siteInfo.SiteId}");

        await sendGridCollector.AddAsync(email).ConfigureAwait(false);
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