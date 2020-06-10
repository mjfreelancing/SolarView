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
  public class TriggerSendPowerSummaryEmail : FunctionBase
  {
    public TriggerSendPowerSummaryEmail(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(TriggerSendPowerSummaryEmail))]
    public async Task Run(
      [TimerTrigger(Constants.Trigger.CronScheduleEveryHour, RunOnStartup = false)] TimerInfo timer,
      [Table(Constants.Table.Sites)] CloudTable sitesTable,
      [ServiceBus(Constants.Queues.SummaryEmail, EntityType.Queue, Connection = Constants.ConnectionStringNames.SolarViewServiceBus)] MessageSender summaryQueue)
    {
      try
      {
        var currentTimeUtc = DateTime.UtcNow;

        Tracker.TrackEvent(nameof(TriggerSendPowerSummaryEmail), new { TriggerTimeUtc = $"{currentTimeUtc.GetSolarDateTimeString()} (UTC)" });

        // determine what sites are due for a power summary email
        var sites = SitesHelpers.GetSites(sitesTable, site =>
        {
          var siteLocalTime = site.UtcToLocalTime(currentTimeUtc);
          return siteLocalTime.Hour == Constants.RefreshHour.SummaryEmail;
        });

        Tracker.TrackInfo(sites.Count == 0
          ? "No sites are due for a power summary email"
          : $"Power summary emails are due for {sites.Count} site(s)");

        if (sites.Count == 0)
        {
          return;
        }

        // create requests for the previous day of each site
        var requests = sites
          .Select(site => new SiteSummaryEmailRequest
          {
            SiteId = site.SiteId,
            LocalDate = $"{site.UtcToLocalTime(currentTimeUtc).Date.AddDays(-1).GetSolarDateString()}"    // only sending yyyy-MM-dd
          });

        // queue of messages
        var queueTasks = requests.Select(request =>
        {
          var message = MessageHelpers.SerializeToMessage(request);

          Tracker.TrackInfo($"Sending a {nameof(SiteSummaryEmailRequest)} message for SiteId {request.SiteId}");

          return summaryQueue.SendAsync(message);
        });

        await Task.WhenAll(queueTasks).ConfigureAwait(false);

        Tracker.TrackInfo("All site power summary messages have been sent");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);

        // todo: send a message to report via email
      }
    }
  }
}