using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using SolarView.Common.Extensions;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Site;
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

        var sitesRepository = _repositoryFactory.Create<ISiteDetailsRepository>(sitesTable);

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

    private async Task ProcessSiteSummaryEmailRequest(DateTime currentTimeUtc, ISiteDetails siteDetails, ISenderClient summaryQueue,
      IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      SiteSummaryEmailRequest request = null;

      try
      {
        var siteLocalTime = siteDetails.UtcToLocalTime(currentTimeUtc);

        // check subsequent hours in case a trigger was missed
        if (siteLocalTime.Hour >= Constants.RefreshHour.SummaryEmail)
        {
          var lastSummaryDate = siteDetails.GetLastSummaryDate();
          var nextEndDate = siteLocalTime.Date.AddDays(-1);         // not reporting the current day as it is not yet over

          if (nextEndDate > lastSummaryDate)
          {
            request = new SiteSummaryEmailRequest
            {
              SiteId = siteDetails.SiteId,
              StartDate = lastSummaryDate.AddDays(1).GetSolarDateString(),
              EndDate = nextEndDate.GetSolarDateString()
            };

            var message = MessageHelpers.SerializeToMessage(request);

            Tracker.TrackInfo(
              $"Sending a {nameof(SiteSummaryEmailRequest)} message for SiteId {request.SiteId} between {request.StartDate} and {request.EndDate}",
              new {Reuqest = request}
            );

            await summaryQueue.SendAsync(message).ConfigureAwait(false);
          }
        }
      }
      catch (Exception exception)
      {
        var notification = new
        {
          siteDetails.SiteId,
          Request = request
        };

        Tracker.TrackException(exception, notification);

        await exceptionDocuments.AddNotificationAsync<TriggerSendPowerSummaryEmail>(siteDetails.SiteId, exception, notification).ConfigureAwait(false);
      }
    }
  }
}