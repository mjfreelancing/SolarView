using AllOverIt.Helpers;
using Microsoft.ApplicationInsights.Extensibility;

namespace SolarCosmosUtil.Telemetry
{
  public class TelemetryTrackerConfiguration : ITelemetryTrackerConfiguration
  {
    public TelemetryConfiguration Configuration { get; }

    public TelemetryTrackerConfiguration()
      : this(TelemetryConfiguration.CreateDefault())
    {
    }

    public TelemetryTrackerConfiguration(TelemetryConfiguration configuration)
    {
      Configuration = configuration.WhenNotNull(nameof(configuration));
    }
  }
}