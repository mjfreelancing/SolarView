using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;

namespace SolarViewFunctions.Functions
{
  public class AggregatePowerYearly : FunctionBase
  {
    public AggregatePowerYearly(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(AggregatePowerYearly))]
    public void Run([ActivityTrigger] IDurableActivityContext context)
    {
      // allowing exceptions to bubble back to the caller

      Tracker.TrackEvent(nameof(AggregatePowerYearly), new {context.InstanceId});

      var request = context.GetInput<SiteRefreshAggregationRequest>();

    }
  }
}