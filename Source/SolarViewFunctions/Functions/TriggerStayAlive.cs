using Microsoft.Azure.WebJobs;
using SolarViewFunctions.Tracking;

namespace SolarViewFunctions.Functions
{
  public class TriggerStayAlive : FunctionBase
  {
    public TriggerStayAlive(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(TriggerStayAlive))]
    public void Run(
      [TimerTrigger(Constants.Trigger.CronScheduleEveryMinute, RunOnStartup = false)] TimerInfo timer)
    {
      Tracker.TrackEvent(nameof(TriggerStayAlive));
    }
  }
}