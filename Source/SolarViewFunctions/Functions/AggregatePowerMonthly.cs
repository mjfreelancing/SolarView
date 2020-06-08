using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;

namespace SolarViewFunctions.Functions
{
  public class AggregatePowerMonthly : FunctionBase
  {
    public AggregatePowerMonthly(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(AggregatePowerMonthly))]
    public void Run([ActivityTrigger] IDurableActivityContext context)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.TrackEvent(nameof(AggregatePowerMonthly), new {context.InstanceId});

      var request = context.GetInput<SiteRefreshAggregationRequest>();


    }
  }
}