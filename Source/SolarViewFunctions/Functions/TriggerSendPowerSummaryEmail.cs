using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
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
      [ServiceBus(Constants.Queues.SummaryEmail, EntityType.Queue, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] MessageSender summaryQueue)
    {
      try
      {
        var currentTimeUtc = DateTime.UtcNow;

        Tracker.TrackEvent(nameof(TriggerSendPowerSummaryEmail), new { TriggerTimeUtc = $"{currentTimeUtc.GetSolarDateTimeString()} (UTC)" });

        var sitesRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);

        var queueTasks = new List<Task>();

        await foreach (var site in sitesRepository.GetAllSitesAsyncEnumerable())
        {
          var siteLocalTime = site.UtcToLocalTime(currentTimeUtc);

          // determine what sites are due for a power summary email
          if (siteLocalTime.Hour == Constants.RefreshHour.SummaryEmail)
          {
            var request = new SiteSummaryEmailRequest
            {
              SiteId = site.SiteId,
              LocalDate = $"{site.UtcToLocalTime(currentTimeUtc).Date.AddDays(-1).GetSolarDateString()}" // only sending yyyy-MM-dd
            };

            var message = MessageHelpers.SerializeToMessage(request);

            Tracker.TrackInfo($"Sending a {nameof(SiteSummaryEmailRequest)} message for SiteId {request.SiteId}");

            var task = summaryQueue.SendAsync(message);

            queueTasks.Add(task);
          }
        }

        if (queueTasks.Count == 0)
        {
          Tracker.TrackInfo("No sites are due for a power summary email");
        }
        else
        {
          await Task.WhenAll(queueTasks).ConfigureAwait(false);

          Tracker.TrackInfo("All site power summary messages have been sent");
        }
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);

        // todo: send a message to report via email
      }
    }
  }
}