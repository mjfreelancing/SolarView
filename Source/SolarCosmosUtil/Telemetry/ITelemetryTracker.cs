using Microsoft.ApplicationInsights;

namespace SolarCosmosUtil.Telemetry
{
  public interface ITelemetryTracker
  {
    TelemetryClient Client { get; }
  }
}