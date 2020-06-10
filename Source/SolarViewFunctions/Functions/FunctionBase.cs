using AllOverIt.Helpers;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Tracking;
using System;

namespace SolarViewFunctions.Functions
{
  public abstract class FunctionBase
  {
    private IRetryOptionsFactory _retryOptionsFactory;
    protected ITracker Tracker { get; private set; }

    protected FunctionBase(ITracker tracker)
    {
      Tracker = tracker.WhenNotNull(nameof(tracker));

      Tracker.AppendDefaultProperties(new {FunctionTag = "SolarView"});
    }

    protected FunctionBase(IRetryOptionsFactory retryOptionsFactory, ITracker tracker)
      : this(tracker)
    {
      // _retryOptionsFactory can be null (when the other constructor is called)
      // - but it shouldn't be null if this constructor is explicitly called
      _retryOptionsFactory = retryOptionsFactory.WhenNotNull(nameof(retryOptionsFactory));
    }

    protected void MakeTrackerReplaySafe(IDurableOrchestrationContext context)
    {
      if (Tracker is ReplaySafeTracker)
      {
        return;
      }

      Tracker = new ReplaySafeTracker(context, Tracker);
    }

    protected RetryOptions GetDefaultRetryOptions(Func<Exception, bool> handler = null)
    {
      _ = _retryOptionsFactory.WhenNotNull(nameof(_retryOptionsFactory));

      var options = _retryOptionsFactory.CreateFixedIntervalRetryOptions(TimeSpan.FromSeconds(15), 3, Tracker);
      options.Handle = handler;

      return options;
    }
  }
}