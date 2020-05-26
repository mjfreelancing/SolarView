using AllOverIt.Helpers;
using AllOverIt.Tasks;
using Microsoft.ApplicationInsights;
using SolarCosmosUtil.Configuration;
using SolarCosmosUtil.KeyVault;

namespace SolarCosmosUtil.Telemetry
{
  public class TelemetryTracker : ITelemetryTracker
  {
    private readonly AsyncLazy<TelemetryClient> _telemetryClient;

    public TelemetryClient Client => _telemetryClient.Value.GetAwaiter().GetResult();

    public TelemetryTracker(ITelemetryTrackerConfiguration trackerConfiguration, IApplicationInsightsConfiguration insightsConfiguration, IKeyVaultCache keyVaultCache)
    {
      _ = trackerConfiguration.WhenNotNull(nameof(trackerConfiguration));
      _ = insightsConfiguration.WhenNotNull(nameof(insightsConfiguration));
      _ = keyVaultCache.WhenNotNull(nameof(keyVaultCache));

      _telemetryClient = new AsyncLazy<TelemetryClient>(async () =>
      {
        var telemetrySecretName = insightsConfiguration.KeyVaultSecretName;
        var instrumentationKey = await keyVaultCache.GetSecretAsync(telemetrySecretName).ConfigureAwait(false);

        trackerConfiguration.Configuration.InstrumentationKey = instrumentationKey;

        return new TelemetryClient(trackerConfiguration.Configuration);
      });
    }
  }
}