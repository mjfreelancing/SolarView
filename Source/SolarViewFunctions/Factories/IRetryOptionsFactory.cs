using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Tracking;
using System;

namespace SolarViewFunctions.Factories
{
  public interface IRetryOptionsFactory
  {
    RetryOptions CreateFixedIntervalRetryOptions(TimeSpan firstRetryInterval, int maxNumberOfAttempts, ITracker tracker, Func<Exception, bool> handler = null);
  }
}