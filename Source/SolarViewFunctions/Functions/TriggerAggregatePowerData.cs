using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerAggregatePowerData : FunctionBase
  {
    public TriggerAggregatePowerData(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(TriggerAggregatePowerData))]
    public async Task Run(
      [TimerTrigger(Constants.Trigger.CronScheduleEveryHour, RunOnStartup = false)] TimerInfo timer,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      try
      {
        Tracker.TrackEvent(nameof(TriggerAggregatePowerData), new { TriggerTimeUtc = $"{DateTime.UtcNow.GetSolarDateTimeString()} (UTC)" });

        // sequentially performs weekly, monthly, yearly aggregation
        var instanceId = await orchestrationClient.StartNewAsync(nameof(AggregatePowerData));

        Tracker.TrackInfo($"Started {nameof(AggregatePowerData)}", new {InstanceId = instanceId});
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception.UnwrapFunctionException());
      }
    }
  }
}