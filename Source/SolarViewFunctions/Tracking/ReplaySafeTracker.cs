using AllOverIt.Helpers;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;

namespace SolarViewFunctions.Tracking
{
  public class ReplaySafeTracker : ITracker
  {
    private readonly IDurableOrchestrationContext _context;
    private readonly ITracker _tracker;
    private bool IsNotReplaying => !_context.IsReplaying;

    public ReplaySafeTracker(IDurableOrchestrationContext context, ITracker tracker)
    {
      _context = context.WhenNotNull(nameof(context));
      _tracker = tracker.WhenNotNull(nameof(tracker));
    }

    public void SetDefaultProperties(object properties)
    {
      throw new InvalidOperationException($"Call {nameof(SetDefaultProperties)}() on the tracker being decorated");
    }

    public void TrackDebug(string message, object properties = null)
    {
      if (IsNotReplaying)
      {
        _tracker.TrackDebug(message, properties);
      }
    }

    public void TrackInfo(string message, object properties = null)
    {
      if (IsNotReplaying)
      {
        _tracker.TrackInfo(message, properties);
      }
    }

    public void TrackWarn(string message, object properties = null)
    {
      if (IsNotReplaying)
      {
        _tracker.TrackWarn(message, properties);
      }
    }

    public void TrackError(string message, object properties = null)
    {
      if (IsNotReplaying)
      {
        _tracker.TrackError(message, properties);
      }
    }

    public void TrackEvent(string message, object properties = null, object metrics = null)
    {
      if (IsNotReplaying)
      {
        _tracker.TrackEvent(message, properties, metrics);
      }
    }

    public void TrackException(Exception exception, object properties = null, object metrics = null)
    {
      if (IsNotReplaying)
      {
        _tracker.TrackException(exception, properties, metrics);
      }
    }
  }
}