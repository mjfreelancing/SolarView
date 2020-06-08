using System;

namespace SolarViewFunctions.Tracking
{
  // todo: create another tracker that reports errors and exceptions via email
  // use a decorator based registration

  public interface ITracker
  {
    void SetDefaultProperties(object properties);

    void TrackDebug(string message, object properties = null);
    void TrackInfo(string message, object properties = null);
    void TrackWarn(string message, object properties = null);
    void TrackError(string message, object properties = null);

    // metrics is expected to be an anonymous object with properties containing double values
    void TrackEvent(string message, object properties = null, object metrics = null);
    void TrackException(Exception exception, object properties = null, object metrics = null);
  }
}