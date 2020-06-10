using AllOverIt.Extensions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerSitesRefreshPowerData : FunctionBase
  {
    public TriggerSitesRefreshPowerData(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(TriggerSitesRefreshPowerData))]
    public async Task Run(
      [TimerTrigger(Constants.Trigger.CronScheduleEveryHour, RunOnStartup = false)] TimerInfo timer,
      [Table(Constants.Table.Sites)] CloudTable sitesTable,
      [ServiceBus(Constants.Queues.SolarPower, EntityType.Queue, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] MessageSender refreshQueue)
    {
      try
      {
        var currentTimeUtc = DateTime.UtcNow;

        Tracker.TrackEvent(nameof(TriggerSitesRefreshPowerData), new {RefreshTimeUtc = currentTimeUtc});

        var requests = SitesHelpers
          .GetSites(sitesTable, site => currentTimeUtc >= site.GetNextRefreshDueUtc())
          .Select(site =>
          {
            var requestTimeUtc = currentTimeUtc.AddSeconds(-currentTimeUtc.Second);

            return new SiteRefreshPowerRequest
            {
              SiteId = site.SiteId,
              DateTime = site.UtcToLocalTime(requestTimeUtc).GetSolarDateTimeString()
            };
          })
          .AsReadOnlyList();

        Tracker.TrackInfo(requests.Count == 0
          ? "No sites are due for a refresh of solar power data"
          : $"Determined {requests.Count} site(s) are due for a refresh of solar power data");

        if (requests.Count == 0)
        {
          return;
        }

        var queueTasks = requests.Select(request =>
        {
          var message = MessageHelpers.SerializeToMessage(request);

          Tracker.TrackInfo($"Sending a {nameof(SiteRefreshPowerRequest)} message for SiteId {request.SiteId}, DateTime {request.DateTime}");

          return refreshQueue.SendAsync(message);
        });

        await Task.WhenAll(queueTasks);

        Tracker.TrackInfo("All site power refresh messages have been sent");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);

        // todo: send a message to report via email
      }
    }
  }
}