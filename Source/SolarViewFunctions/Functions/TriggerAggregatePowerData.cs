using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerAggregatePowerData : FunctionBase
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public TriggerAggregatePowerData(ITracker tracker, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(TriggerAggregatePowerData))]
    public async Task Run(
      [TimerTrigger(Constants.Trigger.CronScheduleEveryHour, RunOnStartup = false)] TimerInfo timer,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      try
      {
        var currentTimeUtc = DateTime.UtcNow;
        Tracker.TrackEvent(nameof(TriggerAggregatePowerData));

        var siteRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);

        var allSites = siteRepository.GetAllSitesAsyncEnumerable();

        await foreach (var siteInfo in allSites)
        {
          try
          {
            if (siteInfo.UtcToLocalTime(currentTimeUtc).Hour == Constants.RefreshHour.Aggregation)
            {
              var (startDate, endDate) = siteInfo.GetNextAggregationPeriod(siteInfo.UtcToLocalTime(currentTimeUtc).Date);

              if (endDate > startDate)
              {
                var request = new SiteRefreshAggregationRequest
                {
                  SiteId = siteInfo.SiteId,
                  SiteStartDate = siteInfo.StartDate,
                  StartDate = startDate.GetSolarDateString(),
                  EndDate = endDate.GetSolarDateString()
                };

                // sequentially performs weekly, monthly, yearly aggregation
                var instanceId = await orchestrationClient.StartNewAsync(nameof(AggregateSitePowerData), request);

                Tracker.TrackInfo($"Power data aggregation for SiteId {siteInfo.SiteId} has been scheduled", new { InstanceId = instanceId });
              }
            }
          }
          catch (Exception exception)
          {
            Tracker.TrackException(exception);

            await exceptionDocuments.AddNotificationAsync<TriggerAggregatePowerData>(siteInfo.SiteId, exception, null);
          }
        }

        Tracker.TrackInfo("All sites have been processed for data aggregation requests");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);
      }
    }
  }
}