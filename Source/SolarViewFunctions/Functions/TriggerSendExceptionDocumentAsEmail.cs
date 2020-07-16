using AllOverIt.Helpers;
using HtmlBuilders;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Site;
using SolarViewFunctions.SendGrid;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerSendExceptionDocumentAsEmail : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;
    private readonly ISendGridEmailCreator _emailCreator;

    public TriggerSendExceptionDocumentAsEmail(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory, ISendGridEmailCreator emailCreator)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
      _emailCreator = emailCreator.WhenNotNull(nameof(emailCreator));
    }

    [FunctionName(nameof(TriggerSendExceptionDocumentAsEmail))]
    public async Task Run(
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [CosmosDBTrigger(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos,
        LeaseCollectionName = Constants.Cosmos.ExceptionLeases)] IReadOnlyList<Document> exceptionDocuments,
      [SendGrid(ApiKey = "SolarViewSendGridApiKey")] IAsyncCollector<SendGridMessage> sendGridCollector)
    {
      Tracker.TrackEvent(nameof(TriggerSendExceptionDocumentAsEmail));
      Tracker.TrackInfo($"Received {exceptionDocuments.Count} documents to send as emails");

      foreach (var document in exceptionDocuments)
      {
        try
        {
          ExceptionDocument exceptionDocument = (dynamic)document;

          var siteInfo = await _repositoryFactory.Create<ISiteDetailsRepository>(sitesTable).GetSiteAsync(exceptionDocument.SiteId);

          var emailContent = BuildHtml(exceptionDocument);
          var email = _emailCreator.CreateMessage(siteInfo, "SolarView Exception Report", "text/html", emailContent);

          await sendGridCollector.AddAsync(email).ConfigureAwait(false);
          await sendGridCollector.FlushAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
          Tracker.TrackException(exception);
        }
      }
    }

    private static string BuildHtml(ExceptionDocument doc)
    {
      var table = CreateTable(doc);

      var body = HtmlTags.Body
        .Style("width", "800")
        .Style("margin-left", "auto")
        .Style("margin-right", "auto")
        .Append(table);

      return HtmlTags.Html
        .Append(body)
        .ToHtmlString();
    }

    private static HtmlTag CreateTable(ExceptionDocument doc)
    {
      var table = HtmlTags.Table
        .Style("border-collapse", "collapse")
        .Attribute("cellpadding", "8")
        .Attribute("cellspacing", "8")
        .Attribute("border", "1");

      table = table.Append(
        HtmlTags.Tr.Append(
          HtmlTags.Td.Append(HtmlTags.Strong.Append("Id")),
          HtmlTags.Td.Append(doc.Id)
        ),

        HtmlTags.Tr.Append(
          HtmlTags.Td.Append(HtmlTags.Strong.Append("SiteId")),
          HtmlTags.Td.Append(doc.SiteId)
        ),

        HtmlTags.Tr.Append(
          HtmlTags.Td.Append(HtmlTags.Strong.Append("Timestamp")),
          HtmlTags.Td.Append($"{doc.Timestamp:yyyy-MM-dd}")
        ),

        HtmlTags.Tr.Append(
          HtmlTags.Td.Append(HtmlTags.Strong.Append("Source")),
          HtmlTags.Td.Append(doc.Source)
        ),

        HtmlTags.Tr.Append(
          HtmlTags.Td.Append(HtmlTags.Strong.Append("Exception")),
          HtmlTags.Td.Append(doc.Exception)
        ),

        HtmlTags.Tr.Append(
          HtmlTags.Td.Append(HtmlTags.Strong.Append("Notification")),
          HtmlTags.Td.Append(HtmlTags.Pre.Append(JsonConvert.SerializeObject(doc.Notification, Formatting.Indented)))
        )
      );

      return table;
    }
  }
}