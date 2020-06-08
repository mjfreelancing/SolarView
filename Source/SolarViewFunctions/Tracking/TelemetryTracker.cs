using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarViewFunctions.Tracking
{
  public class TelemetryTracker : ITracker
  {
    private IDictionary<string, string> _defaultProperties;
    private readonly TelemetryClient _telemetryClient;

    public TelemetryTracker(TelemetryClient telemetryClient)
    {
      _telemetryClient = telemetryClient.WhenNotNull(nameof(telemetryClient));
    }

    public void SetDefaultProperties(object properties)
    {
      if(_defaultProperties != null)
      {
        throw new InvalidOperationException($"Cannot call {nameof(SetDefaultProperties)}() more than once");
      }

      _defaultProperties = GetProperties(properties);
    }

    public void TrackDebug(string message, object properties = null)
    {
      _telemetryClient.TrackTrace(message, SeverityLevel.Verbose, GetProperties(properties));
    }

    public void TrackInfo(string message, object properties = null)
    {
      _telemetryClient.TrackTrace(message, SeverityLevel.Information, GetProperties(properties));
    }

    public void TrackWarn(string message, object properties = null)
    {
      _telemetryClient.TrackTrace(message, SeverityLevel.Warning, GetProperties(properties));
    }

    public void TrackError(string message, object properties = null)
    {
      _telemetryClient.TrackTrace(message, SeverityLevel.Error, GetProperties(properties));
    }

    public void TrackEvent(string message, object properties = null, object metrics = null)
    {
      _telemetryClient.TrackEvent(message, GetProperties(properties), GetMetrics(metrics));
    }

    public void TrackException(Exception exception, object properties = null, object metrics = null)
    {
      _telemetryClient.TrackException(exception, GetProperties(properties), GetMetrics(metrics));
    }

    private IDictionary<string, string> GetProperties(object properties)
    {
      if (properties == null)
      {
        return _defaultProperties;
      }

      var propInfos = from prop in properties.GetType().GetPropertyInfo()
        where prop.CanRead
        let value = prop.GetValue(properties)
        select new KeyValuePair<string, string>(prop.Name, $"{value}");

      var objectProperties = propInfos.ToDictionary(item => item.Key, item => item.Value);

      if (_defaultProperties != null)
      {
        foreach (var (key, value) in _defaultProperties)
        {
          if (!objectProperties.ContainsKey(key))
          {
            objectProperties.Add(key, value);
          }
        }
      }

      return objectProperties;
    }

    private static IDictionary<string, double> GetMetrics(object metrics)
    {
      if (metrics == null)
      {
        return default;
      }

      var propInfos = from prop in metrics.GetType().GetPropertyInfo()
        where prop.CanRead
        let value = prop.GetValue(metrics)
        select new KeyValuePair<string, double>(prop.Name, value.As<double>());

      return propInfos.ToDictionary(item => item.Key, item => item.Value);
    }
  }
}