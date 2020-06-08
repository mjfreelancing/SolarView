using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;

namespace SolarViewFunctions.Functions
{
  public class AggregatePowerWeekly : FunctionBase
  {
    public AggregatePowerWeekly(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(AggregatePowerWeekly))]
    public void Run([ActivityTrigger] IDurableActivityContext context)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.TrackEvent(nameof(AggregatePowerWeekly), new {context.InstanceId});

      var request = context.GetInput<SiteRefreshAggregationRequest>();


    }
  }
}