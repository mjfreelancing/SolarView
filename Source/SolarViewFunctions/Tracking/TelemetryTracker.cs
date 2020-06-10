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

    public void AppendDefaultProperties(object properties)
    {
      // merges 'properties' into '_defaultProperties' and re-assigns
      _defaultProperties = GetCombinedProperties(properties);
    }

    public void TrackDebug(string message, object properties = null)
    {
      _telemetryClient.TrackTrace(message, SeverityLevel.Verbose, GetCombinedProperties(properties));
    }

    public void TrackInfo(string message, object properties = null)
    {
      _telemetryClient.TrackTrace(message, SeverityLevel.Information, GetCombinedProperties(properties));
    }

    public void TrackWarn(string message, object properties = null)
    {
      _telemetryClient.TrackTrace(message, SeverityLevel.Warning, GetCombinedProperties(properties));
    }

    public void TrackError(string message, object properties = null)
    {
      _telemetryClient.TrackTrace(message, SeverityLevel.Error, GetCombinedProperties(properties));
    }

    public void TrackEvent(string message, object properties = null, object metrics = null)
    {
      _telemetryClient.TrackEvent(message, GetCombinedProperties(properties), GetMetrics(metrics));
    }

    public void TrackException(Exception exception, object properties = null, object metrics = null)
    {
      _telemetryClient.TrackException(exception, GetCombinedProperties(properties), GetMetrics(metrics));
    }

    private IDictionary<string, string> GetCombinedProperties(object properties)
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
        MergeProperties(_defaultProperties, objectProperties);
      }

      return objectProperties;
    }

    private static void MergeProperties(IDictionary<string, string> source, IDictionary<string, string> destination)
    {
      foreach (var (key, value) in source)
      {
        if (!destination.ContainsKey(key))
        {
          destination.Add(key, value);
        }
      }
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