using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
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
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      try
      {
        var currentTimeUtc = DateTime.UtcNow;
        Tracker.TrackEvent(nameof(TriggerAggregatePowerData), new { TriggerTimeUtc = $"{currentTimeUtc.GetSolarDateTimeString()} (UTC)" });

        var siteRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);

        var allSites = siteRepository.GetAllSitesAsyncEnumerable();

        var aggregationTasks = new List<(string SiteId, Task<string> SiteTask)>();

        await foreach (var site in allSites)
        {
          if (site.UtcToLocalTime(currentTimeUtc).Hour == Constants.RefreshHour.Aggregation)
          {
            var (startDate, endDate) = site.GetNextAggregationPeriod(site.UtcToLocalTime(currentTimeUtc).Date);

            if (endDate > startDate)
            {
              var request = new SiteRefreshAggregationRequest
              {
                SiteId = site.SiteId,
                SiteStartDate = site.StartDate,
                StartDate = startDate.GetSolarDateString(),
                EndDate = endDate.GetSolarDateString()
              };

              // sequentially performs weekly, monthly, yearly aggregation
              var task = orchestrationClient.StartNewAsync(nameof(AggregateSitePowerData), request);

              aggregationTasks.Add((request.SiteId, task));
            }
          }
        }

        if (aggregationTasks.Count == 0)
        {
          Tracker.TrackInfo("No sites are due for a power data aggregation");
        }
        else
        {
          Tracker.TrackInfo($"Power data aggregation has begun for {aggregationTasks.Count} site(s)");

          var tasks = aggregationTasks.Select(item => item.SiteTask);
          await Task.WhenAll(tasks).ConfigureAwait(false);

          foreach (var (siteId, siteTask) in aggregationTasks)
          {
            Tracker.TrackInfo($"Power data aggregation for SiteId {siteId} has been scheduled", new {InstanceId = siteTask.Result});
          }
        }
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception.UnwrapFunctionException());
      }
    }
  }
}