using AllOverIt.Helpers;
using Microsoft.ApplicationInsights;
using SolarCosmosUtil.Configuration;
using SolarCosmosUtil.KeyVault;
using System;

namespace SolarCosmosUtil.Telemetry
{
  public class TelemetryTracker : ITelemetryTracker
  {
    private readonly Lazy<TelemetryClient> _telemetryClient;

    // todo: remove this and replace with interface methods that are proxied to _telemetryClient
    // ITelemetryTracker is currently registered as a singleton but it would be better to make _telemetryClient static
    // and then handle any situation where it needs to be recreated because of key vault issues.
    // Can then add methods that allow anonymous objects to be passed as properties instead of a Dictionary.
    public TelemetryClient Client => _telemetryClient.Value;

    public TelemetryTracker(ITelemetryTrackerConfiguration trackerConfiguration, IApplicationInsightsConfiguration insightsConfiguration, IKeyVaultCache keyVaultCache)
    {
      _ = trackerConfiguration.WhenNotNull(nameof(trackerConfiguration));
      _ = insightsConfiguration.WhenNotNull(nameof(insightsConfiguration));
      _ = keyVaultCache.WhenNotNull(nameof(keyVaultCache));

      _telemetryClient = new Lazy<TelemetryClient>(() =>
      {
        var telemetrySecretName = insightsConfiguration.KeyVaultSecretName;
        var instrumentationKey = keyVaultCache.GetSecret(telemetrySecretName);

        trackerConfiguration.Configuration.InstrumentationKey = instrumentationKey;

        return new TelemetryClient(trackerConfiguration.Configuration);
      });
    }
  }
}