using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;

namespace SolarCosmosUtil.Telemetry
{
  public static class TelemetryConfigurationExtensions
  {
    public static DependencyTrackingTelemetryModule UseDependencyTrackingTelemetryModule(this TelemetryConfiguration configuration)
    {
      // https://docs.microsoft.com/en-us/azure/azure-monitor/app/console
      var module = new DependencyTrackingTelemetryModule();

      // prevent Correlation Id to be sent to certain endpoints.
      module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.windows.net");
      module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.chinacloudapi.cn");
      module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.cloudapi.de");
      module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("core.usgovcloudapi.net");
      module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("localhost");
      module.ExcludeComponentCorrelationHttpHeadersOnDomains.Add("127.0.0.1");

      // enable known dependency tracking, note that in future versions, we will extend this list. 
      // please check default settings in https://github.com/microsoft/ApplicationInsights-dotnet-server/blob/develop/WEB/Src/DependencyCollector/DependencyCollector/ApplicationInsights.config.install.xdt

      module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.ServiceBus");
      module.IncludeDiagnosticSourceActivities.Add("Microsoft.Azure.EventHubs");

      // initialize the module
      module.Initialize(configuration);

      return module;
    }
  }
}