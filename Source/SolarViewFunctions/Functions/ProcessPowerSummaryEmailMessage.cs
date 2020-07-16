using AllOverIt.Extensions;
using AllOverIt.Helpers;
using HtmlBuilders;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using SendGrid.Helpers.Mail;
using SolarView.Common.Extensions;
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
using System.Collections.Generic;
using System.Linq;
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

        var siteInfo = await _repositoryFactory.Create<ISiteDetailsRepository>(sitesTable).GetSiteAsync(request.SiteId);

        var updateHistoryRepository = _repositoryFactory.Create<IPowerUpdateHistoryRepository>(updateHistoryTable);

        var historyItems = await updateHistoryRepository.GetPowerUpdatesAsyncEnumerable(request.SiteId,
          request.StartDate.ParseSolarDate(), request.EndDate.ParseSolarDate());

        var emailContent = BuildHtml(historyItems);

        Tracker.TrackInfo($"Sending summary email for SiteId {siteInfo.SiteId}");

        var email = _emailCreator.CreateMessage(siteInfo, "Power Update Summary", "text/html", $"{emailContent}");
        await sendGridCollector.AddAsync(email).ConfigureAwait(false);
        await sendGridCollector.FlushAsync().ConfigureAwait(false);

        // update the sites table to indicate when the last summary email was sent
        var updateProperties = new Dictionary<string, object> {{nameof(ISiteDetails.LastSummaryDate), request.EndDate}};
        await _sitesUpdateProvider.UpdateSiteAttributeAsync(sitesTable, siteInfo.SiteId, updateProperties);
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

    private static string BuildHtml(IEnumerable<PowerUpdateEntity> historyItems)
    {
      var historyGroups = historyItems
        .Where(item => item.Status != $"{PowerUpdatedStatus.Started}")
        .GroupBy(item => item.TriggerType);

      var tables = new List<HtmlTag>();

      foreach (var kvp in historyGroups)
      {
        var trigger = kvp.Key;
        var items = kvp.OrderBy(item => item.TriggerDateTime).AsReadOnlyList();

        var table = CreateTable(trigger, items);

        var div = HtmlTags.Div
          .Style("margin-bottom", "24")
          .Append(table);

        tables.Add(div);
      }

      var body = HtmlTags.Body
        .Style("width", "640")
        .Style("margin-left", "auto")
        .Style("margin-right", "auto")
        .Append(tables);

      return HtmlTags.Html
        .Append(body)
        .ToHtmlString();
    }

    private static HtmlTag CreateTable(string trigger, IEnumerable<PowerUpdateEntity> items)
    {
      var table = HtmlTags.Table
        .Style("width", "640")
        .Style("border-collapse", "collapse")
        .Attribute("cellpadding", "4")
        .Attribute("cellspacing", "4")
        .Attribute("border", "1");

      var tableHeader = HtmlTags.Tr.Append(
        HtmlTags.Th.Attribute("colspan", "4")
          .Style("background-color", "#404040")
          .Style("color", "white")
          .Style("padding", "8")
          .Style("margin-bottom", "4")
          .Append($"Trigger: {trigger}")
      );

      table = table.Append(tableHeader);

      table = table.Append(CreateColumnHeaders());

      foreach (var item in items)
      {
        var row = HtmlTags.Tr.Style("text-align", "center");

        if (item.Status == $"{PowerUpdatedStatus.Error}")
        {
          row = row.Style("background-color", "red").Style("color", "white");
        }

        row = row.Append(
          HtmlTags.Td.Append(item.TriggerDateTime),
          HtmlTags.Td.Append(item.StartDateTime),
          HtmlTags.Td.Append(item.EndDateTime),
          HtmlTags.Td.Append(item.Status)
        );

        table = table.Append(row);
      }

      return table;
    }

    private static HtmlTag CreateColumnHeaders()
    {
      return HtmlTags.Tr
        .Style("text-align", "center")
        .Style("background-color", "#457b9d").Style("color", "white")
        .Append(
          HtmlTags.Td.Append(HtmlTags.Strong.Append("Triggered")),
          HtmlTags.Td.Append(HtmlTags.Strong.Append("Start Time")),
          HtmlTags.Td.Append(HtmlTags.Strong.Append("End Time")),
          HtmlTags.Td.Append(HtmlTags.Strong.Append("Status"))
        );
    }
  }
}