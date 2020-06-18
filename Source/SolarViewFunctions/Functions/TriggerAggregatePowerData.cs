using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Site;
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

        var siteRepository = _repositoryFactory.Create<ISiteRepository>(sitesTable);

        var allSites = siteRepository.GetAllSitesAsyncEnumerable();

        await foreach (var siteInfo in allSites)
        {
          await ProcessAggregatePowerRequest(currentTimeUtc, siteInfo, orchestrationClient, exceptionDocuments).ConfigureAwait(false);
        }

        Tracker.TrackInfo("Data aggregation requests have been scheduled for all sites");
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);
      }
    }

    private async Task ProcessAggregatePowerRequest(DateTime currentTimeUtc, ISiteInfo siteInfo, IDurableOrchestrationClient orchestrationClient,
      IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      try
      {
        var siteLocalTime = siteInfo.UtcToLocalTime(currentTimeUtc);

        // check subsequent hours in case a trigger was missed
        if (siteLocalTime.Hour >= Constants.RefreshHour.Aggregation)
        {
          var lastAggregationDate = siteInfo.GetLastAggregationDate();
          var nextEndDate = siteLocalTime.Date.AddDays(-1);         // not reporting the current day as it is not yet over

          if (nextEndDate > lastAggregationDate)
          {
            var request = new SiteRefreshAggregationRequest
            {
              SiteId = siteInfo.SiteId,
              SiteStartDate = siteInfo.StartDate,
              StartDate = lastAggregationDate.GetSolarDateString(),
              EndDate = nextEndDate.GetSolarDateString()
            };

            // sequentially performs monthly then yearly aggregation
            var instanceId = await orchestrationClient.StartNewAsync(nameof(AggregateSitePowerData), request).ConfigureAwait(false);

            Tracker.TrackInfo(
              $"Power data aggregation for SiteId {siteInfo.SiteId} has been scheduled for {request.StartDate} to {request.EndDate}",
              new {Request = request, InstanceId = instanceId});
          }
        }
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception);

        await exceptionDocuments.AddNotificationAsync<TriggerAggregatePowerData>(siteInfo.SiteId, exception, null).ConfigureAwait(false);
        await exceptionDocuments.FlushAsync().ConfigureAwait(false);
      }
    }
  }
}