﻿namespace SolarViewFunctions
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
    }

    public static class RefreshHour
    {
      public const int SummaryEmail = 1;
      public const int Aggregation = 2;
    }

    public static class Orchestration
    {
      public const int HistoryDaysToKeep = 2;
    }

    public static class Trigger
    {
      // Cheat sheet: https://arminreiter.com/2017/02/azure-functions-time-trigger-cron-cheat-sheet/
      public const string CronScheduleEveryHour = "0 0 */1 * * *";
      public const string CronScheduleEveryMinute = "0 */1 * * * *";      // (for local testing)
    }
  }
}