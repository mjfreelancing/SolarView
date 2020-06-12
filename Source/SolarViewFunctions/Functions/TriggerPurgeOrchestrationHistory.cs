using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Extensions;
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
      try
      {
        var currentTimeUtc = DateTime.UtcNow;

        Tracker.TrackEvent(nameof(TriggerPurgeOrchestrationHistory));

        var purgeDate = currentTimeUtc.AddDays(-Constants.Orchestration.HistoryDaysToKeep);
        var statuses = new[] { OrchestrationStatus.Completed, OrchestrationStatus.Failed, OrchestrationStatus.Terminated };

        Tracker.TrackInfo($"Requesting to purge orchestration instances prior to {purgeDate.GetSolarDateTimeString()} (UTC)");

        var purgeHistory = await orchestrationClient.PurgeInstanceHistoryAsync(DateTime.MinValue, purgeDate, statuses);

        Tracker.TrackInfo($"Purged {purgeHistory.InstancesDeleted} orchestration instances prior to {purgeDate.GetSolarDateTimeString()} (UTC)");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);

        // can't email any exceptions since this is not for a specific site
      }
    }
  }
}