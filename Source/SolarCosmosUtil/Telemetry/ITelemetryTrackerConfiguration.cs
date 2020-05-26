using Microsoft.ApplicationInsights.Extensibility;

namespace SolarCosmosUtil.Telemetry
{
  public interface ITelemetryTrackerConfiguration
  {
    public TelemetryConfiguration Configuration { get; }
  }
}