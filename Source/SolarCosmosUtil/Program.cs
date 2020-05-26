using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolarCosmosUtil.Configuration;
using SolarCosmosUtil.Entities;
using SolarCosmosUtil.Extensions;
using SolarCosmosUtil.KeyVault;
using SolarCosmosUtil.Logging;
using SolarCosmosUtil.Repository;
using SolarCosmosUtil.SolarEdge;
using SolarCosmosUtil.Telemetry;
using System;
using System.Threading.Tasks;

namespace SolarCosmosUtil
{


  class Program
  {
    static async Task Main(string[] args)
    {
      var serviceCollection = new ServiceCollection();

      ConfigureServices(serviceCollection);

      var serviceProvider = serviceCollection.BuildServiceProvider();

      var telemetryTracker = serviceProvider.GetRequiredService<ITelemetryTracker>();
      var logger = serviceProvider.GetRequiredService<IApplicationLogger>();
      var trackerConfiguration = serviceProvider.GetRequiredService<ITelemetryTrackerConfiguration>();
      var solarEdgeConfiguration = serviceProvider.GetRequiredService<ISolarEdgeConfiguration>();
      var solarApi = serviceProvider.GetRequiredService<ISolarEdgeServiceClient>();
      var cosmosClient = serviceProvider.GetRequiredService<ISolarViewCosmosDb>();

      try
      {
        var siteId = solarEdgeConfiguration.SiteId;

        using (trackerConfiguration.Configuration.UseDependencyTrackingTelemetryModule())
        {
          telemetryTracker.Client.TrackTrace($"Starting SolarView Console for site {siteId}");

          logger.Debug<Program>("Getting data from SolarEdge");

          var startDate = new DateTime(2020, 5, 19, 0, 0, 0);
          var endDate = DateTime.Now;

          var solarDays = solarApi.GetSolarDataAsync(startDate, endDate);

          await foreach (var solarDay in solarDays)
          {
            var solarDoc = new SolarDocument(siteId, solarDay);

            var response = await cosmosClient.UpsertSolarDocumentAsync(solarDoc).ConfigureAwait(false);

            logger.Debug<Program>($"Data stored for {solarDay.Date:dd-MM-yyyy}, RU={response.RequestCharge}");
          }
        }

        telemetryTracker.Client.TrackTrace($"Ending SolarView Console for site {siteId}");
        telemetryTracker.Client.Flush();

        await Task.Delay(5000).ConfigureAwait(false);

        logger.Debug<Program>("Done");
      }
      catch (CosmosException exception)
      {
        telemetryTracker.Client.TrackException(exception);
      }
      catch (Exception exception)
      {
        telemetryTracker.Client.TrackException(exception);
      }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
      services.AddScoped<IKeyVaultCache,KeyVaultCache>();
      services.AddScoped<IKeyVaultConfiguration, KeyVaultConfiguration>();
      services.AddScoped<ISolarEdgeConfiguration, SolarEdgeConfiguration>();
      services.AddScoped<ICosmosConfiguration, CosmosConfiguration>();
      services.AddScoped<ISolarEdgeServiceClient, SolarEdgeServiceClient>();
      services.AddScoped<ISolarViewCosmosDb, SolarViewCosmosDb>();
      services.AddScoped<IApplicationInsightsConfiguration, ApplicationInsightsConfiguration>();

      var trackerConfiguration = new TelemetryTrackerConfiguration();
      trackerConfiguration.Configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
      services.Add(new ServiceDescriptor(typeof(ITelemetryTrackerConfiguration), trackerConfiguration));

      var applicationConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
      services.Add(new ServiceDescriptor(typeof(IConfiguration), applicationConfig));

      services.AddSingleton<ITelemetryTracker, TelemetryTracker>();
      services.AddSingleton<ILoggingFactory, LoggingFactory>();

      services
        .AddSingleton<IApplicationLogger, ApplicationLogger>()
        .Decorate<IApplicationLogger, ApplicationLoggerWithTelemetry>();

      services
        .AddLogging(builder => builder.AddConsole())
        .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);
    }
  }
}
