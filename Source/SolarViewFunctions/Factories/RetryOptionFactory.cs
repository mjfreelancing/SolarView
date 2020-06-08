using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Tracking;
using System;

namespace SolarViewFunctions.Factories
{
  public class RetryOptionsFactory : IRetryOptionsFactory
  {
    public RetryOptions CreateFixedIntervalRetryOptions(TimeSpan firstRetryInterval, int maxNumberOfAttempts, ITracker tracker, Func<Exception, bool> handler = null)
    {
      return new RetryOptions(firstRetryInterval, maxNumberOfAttempts)
      {
        Handle = exception =>
        {
          tracker.TrackException(exception);
          return handler?.Invoke(exception) ?? true;
        }
      };
    }
  }
}