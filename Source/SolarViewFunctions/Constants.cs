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
      public const string PowerUpdated = "powerupdated";
      public const string SolarPower = "solarpower";
      public const string SummaryEmail = "summaryemail";
      public const string Exception = "exception";
    }

    public static class Table
    {
      public const string Sites = "Sites";
      public const string Power = "Power";
      public const string PowerWeekly = "PowerWeekly";
      public const string PowerMonthly = "PowerMonthly";
      public const string PowerYearly = "PowerYearly";
      public const string PowerUpdateHistory = "PowerUpdateHistory";
    }

    public static class Cosmos
    {
      public const string SolarDatabase = "solar";
      public const string PowerCollection = "power";
      public const string PowerLeases = "powerleases";
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