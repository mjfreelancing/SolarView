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
      [TimerTrigger(Constants.Trigger.CronScheduleEveryMinute, RunOnStartup = false)] TimerInfo timer,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [ServiceBus(Constants.Queues.SolarPower, EntityType.Queue, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] MessageSender refreshQueue)
    {
      try
      {
        var currentTimeUtc = DateTime.UtcNow;

        Tracker.TrackEvent(nameof(TriggerSitesRefreshPowerData), new { TriggerTimeUtc = $"{currentTimeUtc.GetSolarDateTimeString()} (UTC)"});

        var sitesRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);

        var queueTasks = new List<Task>();

        await foreach (var site in sitesRepository.GetAllSitesAsyncEnumerable())
        {
          if (currentTimeUtc >= site.GetNextRefreshDueUtc())
          {
            var requestTimeUtc = currentTimeUtc.AddSeconds(-currentTimeUtc.Second);

            var request = new SiteRefreshPowerRequest
            {
              SiteId = site.SiteId,
              DateTime = site.UtcToLocalTime(requestTimeUtc).GetSolarDateTimeString()
            };

            var message = MessageHelpers.SerializeToMessage(request);

            Tracker.TrackInfo($"Sending a {nameof(SiteRefreshPowerRequest)} message for SiteId {request.SiteId}, DateTime {request.DateTime}");

            var task = refreshQueue.SendAsync(message);

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

          Tracker.TrackInfo("All site power refresh messages have been sent");
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