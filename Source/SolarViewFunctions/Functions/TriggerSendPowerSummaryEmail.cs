using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerSendPowerSummaryEmail : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public TriggerSendPowerSummaryEmail(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(TriggerSendPowerSummaryEmail))]
    public async Task Run(
      [TimerTrigger(Constants.Trigger.CronScheduleEveryHour, RunOnStartup = false)] TimerInfo timer,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [ServiceBus(Constants.Queues.SummaryEmail, EntityType.Queue, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] MessageSender summaryQueue)
    {
      try
      {
        var currentTimeUtc = DateTime.UtcNow;

        Tracker.TrackEvent(nameof(TriggerSendPowerSummaryEmail));

        var sitesRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);

        await foreach (var site in sitesRepository.GetAllSitesAsyncEnumerable())
        {
          await ProcessSiteSummaryEmailRequest(currentTimeUtc, site, summaryQueue, exceptionDocuments).ConfigureAwait(false);
        }

        Tracker.TrackInfo("All sites have been processed for power summary messages");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);
      }
    }

    private async Task ProcessSiteSummaryEmailRequest(DateTime currentTimeUtc, SiteInfo siteInfo, ISenderClient summaryQueue,
      IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      SiteSummaryEmailRequest request = null;

      try
      {
        var siteLocalTime = siteInfo.UtcToLocalTime(currentTimeUtc);

        // determine what sites are due for a power summary email
        if (siteLocalTime.Hour == Constants.RefreshHour.SummaryEmail)
        {
          request = new SiteSummaryEmailRequest
          {
            SiteId = siteInfo.SiteId,
            LocalDate = $"{siteInfo.UtcToLocalTime(currentTimeUtc).Date.AddDays(-1).GetSolarDateString()}" // only sending yyyy-MM-dd
          };

          var message = MessageHelpers.SerializeToMessage(request);

          Tracker.TrackInfo($"Sending a {nameof(SiteSummaryEmailRequest)} message for SiteId {request.SiteId}");

          await summaryQueue.SendAsync(message).ConfigureAwait(false);
        }
      }
      catch (Exception exception)
      {
        var notification = new
        {
          siteInfo.SiteId,
          Request = request
        };

        Tracker.TrackException(exception, notification);

        await exceptionDocuments.AddNotificationAsync<TriggerSendPowerSummaryEmail>(siteInfo.SiteId, exception, notification);
      }
    }
  }
}