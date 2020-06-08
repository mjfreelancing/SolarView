using AllOverIt.Extensions;
using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models;
using SolarViewFunctions.Models.Messages;
using SolarViewFunctions.Tracking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class HydratePowerOrchestrator : FunctionBase
  {
    private readonly IMapper _mapper;

    public HydratePowerOrchestrator(IRetryOptionsFactory retryOptionsFactory, ITracker tracker, IMapper mapper)
      : base(retryOptionsFactory, tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
    }

    [FunctionName(nameof(HydratePowerOrchestrator))]
    public async Task<PowerUpdatedStatus> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
      MakeTrackerReplaySafe(context);

      var powerQuery = context.GetInput<TriggeredPowerQuery>();

      try
      {
        Tracker.TrackInfo(
          $"Preparing to orchestrate power refresh for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}",
          new { context.InstanceId }
        );

        await NotifyPowerUpdated(context, PowerUpdatedStatus.Started, powerQuery);

        // The solarEdge API can only process, at most, 1 month of data at a time
        var dateRanges = SolarViewHelpers
          .GetMonthlyDateRanges(powerQuery.StartDateTime.ParseSolarDateTime(), powerQuery.EndDateTime.ParseSolarDateTime())
          .AsReadOnlyList();

        IEnumerable<Task> GetPowerCollectionTasks()
        {
          foreach (var dateRange in dateRanges)
          {
            // (potentially) prepare a new request for a smaller date range than the original request
            var dateRangeRequest = _mapper.Map<PowerQuery, PowerQuery>(powerQuery);
            dateRangeRequest.StartDateTime = dateRange.StartDateTime.GetSolarDateTimeString();
            dateRangeRequest.EndDateTime = dateRange.EndDateTime.GetSolarDateTimeString();

            Tracker.TrackInfo(
              $"Sub-orchestrating power refresh for SiteId {powerQuery.SiteId} between {dateRangeRequest.StartDateTime} and {dateRangeRequest.EndDateTime}",
              new { context.InstanceId }
            );

            // sub-orchestrate to limit the date ranges processed
            yield return context.CallSubOrchestratorWithRetryAsync(
              nameof(HydratePowerPeriodSubOrchestrator),
              GetDefaultRetryOptions(exception =>
              {
                // don't bother retrying if unable to get the solar data
                if (exception.UnwrapFunctionException() is SolarEdgeResponseException solarEdgeResponse)
                {
                  return solarEdgeResponse.StatusCode != HttpStatusCode.Forbidden && 
                         solarEdgeResponse.StatusCode != HttpStatusCode.TooManyRequests;
                }

                return true;
              }),
              dateRangeRequest);
          }
        }

        var tasks = GetPowerCollectionTasks();

        await Task.WhenAll(tasks);

        Tracker.TrackInfo($"All power data refreshed for SiteId {powerQuery.SiteId}", new { context.InstanceId });

        return await NotifyPowerUpdated(context, PowerUpdatedStatus.Completed, powerQuery);
      }
      catch (Exception exception)
      {
        return await HandleException(exception.UnwrapFunctionException(), context, powerQuery);
      }
    }

    private Task<PowerUpdatedStatus> NotifyPowerUpdated(IDurableOrchestrationContext context, PowerUpdatedStatus status, TriggeredPowerQuery powerQuery)
    {
      var message = _mapper.Map<PowerUpdatedMessage>(powerQuery);
      message.Status = status;

      Tracker.TrackInfo($"Sending '{status}' status notification for SiteId {message.SiteId}, Trigger {message.Trigger}");

      return context.CallActivityWithRetryAsync<PowerUpdatedStatus>(nameof(NotifyPowerUpdateStatusMessage), GetDefaultRetryOptions(), message);
    }

    private async Task<PowerUpdatedStatus> HandleException(Exception exception, IDurableOrchestrationContext context, TriggeredPowerQuery powerQuery)
    {
      Tracker.TrackException(exception, new { context.InstanceId });

      // todo: send a message to send an email

      return await NotifyPowerUpdated(context, PowerUpdatedStatus.Error, powerQuery);
    }
  }
}