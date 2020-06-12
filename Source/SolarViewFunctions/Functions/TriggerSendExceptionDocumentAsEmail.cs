using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
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

          // todo: think about grouping all documents by SiteId and send one email per site

          var siteInfo = await _repositoryFactory.Create<ISitesRepository>(sitesTable).GetSiteAsync(exceptionDocument.SiteId);

          // todo: update to load a specific razor template - just send the model in the email for now
          var content = JsonConvert.SerializeObject(exceptionDocument, Formatting.Indented);

          var email = _emailCreator.CreateMessage(siteInfo, "Exception", "text/plain", content);
          await sendGridCollector.AddAsync(email).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
          Tracker.TrackException(exception);
        }
      }
    }
  }
}