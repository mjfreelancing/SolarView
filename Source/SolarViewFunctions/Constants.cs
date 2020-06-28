namespace SolarViewFunctions
{
  internal static class Constants
  {
    public static class SolarEdge
    {
      public const string MonitoringUri = "https://monitoringapi.solaredge.com/site/";
    }

    public static class Queues
    {
      public const string PowerRefresh = "power-refresh";
      public const string PowerRefreshDeadletter = "power-refresh/$DeadLetterQueue";
      public const string PowerUpdated = "power-updated";
      public const string PowerUpdatedDeadletter = "power-updated/$DeadLetterQueue";
      public const string SummaryEmail = "summary-email";
      public const string SummaryEmailDeadletter = "summary-email/$DeadLetterQueue";
    }

    public static class Table
    {
      public const string Sites = "Sites";
      public const string SitesPartitionKey = "SiteId";
      public const string Power = "Power";
      public const string PowerMonthly = "PowerMonthly";
      public const string PowerYearly = "PowerYearly";
      public const string PowerUpdateHistory = "PowerUpdateHistory";
    }

    public static class Cosmos
    {
      public const string SolarDatabase = "solar";
      public const string ExceptionCollection = "exception";
      public const string ExceptionLeases = "exception-leases";
    }

    public static class ConnectionStringNames
    {
      public const string SolarViewCosmos = "SolarViewCosmosConnectionString";
      public const string SolarViewServiceBus = "SolarViewServiceBusConnectionString";
      public const string SolarViewStorage = "SolarViewStorageConnectionString";
    }

    public static class RefreshHour
    {
      public const int SummaryEmail = 1;    // 1am each day
      public const int Aggregation = 2;     // 2am each day
    }

    public static class Orchestration
    {
      public const int HistoryDaysToKeep = 2;
    }

    public static class Trigger
    {
      // NCronTab: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=csharp#ncrontab-expressions
      public const string CronScheduleEveryHour = "0 0 * * * *";

      // only used for local testing
      public const string CronScheduleEveryMinute = "0 * * * * *";
      public const string CronScheduleEveryFiveMinutes = "0 */5 * * * *";
    }

    public static class AggregationOptions
    {
      public const string CultureName = "en-US";
    }
  }
}