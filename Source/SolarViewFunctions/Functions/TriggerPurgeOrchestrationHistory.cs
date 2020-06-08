using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerPurgeOrchestrationHistory : FunctionBase
  {
    public TriggerPurgeOrchestrationHistory(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(TriggerPurgeOrchestrationHistory))]
    public async Task Run(
      [TimerTrigger(Constants.Trigger.CronScheduleEveryHour, RunOnStartup = false)] TimerInfo timer,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      var currentTimeUtc = DateTime.UtcNow;

      Tracker.TrackEvent(nameof(TriggerPurgeOrchestrationHistory), new { RefreshTimeUtc = currentTimeUtc });

      try
      {
        var purgeDate = currentTimeUtc.AddDays(-Constants.Orchestration.HistoryDaysToKeep);
        var statuses = new[] { OrchestrationStatus.Completed, OrchestrationStatus.Failed, OrchestrationStatus.Terminated };

        Tracker.TrackInfo($"Requesting to purge orchestration instances prior to {purgeDate} (UTC)");

        var purgeHistory = await orchestrationClient.PurgeInstanceHistoryAsync(DateTime.MinValue, purgeDate, statuses);

        Tracker.TrackInfo($"Purged {purgeHistory.InstancesDeleted} orchestration instances prior to {purgeDate} (UTC)");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);
      }
    }
  }
}