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
  public class TriggerSitesRefreshPowerData : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public TriggerSitesRefreshPowerData(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(TriggerSitesRefreshPowerData))]
    public async Task Run(
      [TimerTrigger(Constants.Trigger.CronScheduleEveryHour, RunOnStartup = false)] TimerInfo timer,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [ServiceBus(Constants.Queues.PowerRefresh, EntityType.Queue, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] MessageSender refreshQueue)
    {
      try
      {
        var currentTimeUtc = DateTime.UtcNow;

        Tracker.TrackEvent(nameof(TriggerSitesRefreshPowerData));

        var sitesRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);

        await foreach (var site in sitesRepository.GetAllSitesAsyncEnumerable())
        {
          await ProcessSiteRefreshPowerRequest(currentTimeUtc, site, refreshQueue, exceptionDocuments).ConfigureAwait(false);
        }

        Tracker.TrackInfo("All sites have been processed for power refresh requests");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);
      }
    }

    private async Task ProcessSiteRefreshPowerRequest(DateTime currentTimeUtc, SiteInfo siteInfo, ISenderClient refreshQueue,
      IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      SiteRefreshPowerRequest request = null;

      try
      {
        var siteLocalTime = siteInfo.UtcToLocalTime(currentTimeUtc);

        var lastRefreshDateTime = siteInfo.GetLastRefreshDateTime();
        var nextRefreshDue = lastRefreshDateTime.AddHours(1);

        if (siteLocalTime > nextRefreshDue)
        {
          request = new SiteRefreshPowerRequest
          {
            SiteId = siteInfo.SiteId,
            StartDateTime = lastRefreshDateTime.GetSolarDateTimeString(),
            EndDateTime = siteLocalTime.TrimToHour().GetSolarDateTimeString()
          };

          var message = MessageHelpers.SerializeToMessage(request);

          Tracker.TrackInfo(
            $"Sending a {nameof(SiteRefreshPowerRequest)} message for SiteId {request.SiteId}, from {request.StartDateTime} to {request.EndDateTime}",
            new {siteInfo.SiteId});

          await refreshQueue.SendAsync(message).ConfigureAwait(false);
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

        await exceptionDocuments.AddNotificationAsync<TriggerSitesRefreshPowerData>(siteInfo.SiteId, exception, notification).ConfigureAwait(false);
      }
    }
  }
}