using AllOverIt.Extensions;
using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarView.Common.Extensions;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models;
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
      Tracker.AppendDefaultProperties(context.GetTrackingProperties());

      var triggeredPowerQuery = context.GetInput<TriggeredPowerQuery>();

      try
      {
        Tracker.TrackInfo($"Preparing to orchestrate power refresh for SiteId {triggeredPowerQuery.SiteId} between " +
                          $"{triggeredPowerQuery.StartDateTime} and {triggeredPowerQuery.EndDateTime}");

        await NotifyPowerUpdated(context, PowerUpdatedStatus.Started, triggeredPowerQuery);

        // The solarEdge API can only process, at most, 1 month of data at a time
        var dateRanges = SolarViewHelpers
          .GetMonthlyDateRanges(triggeredPowerQuery.StartDateTime.ParseSolarDateTime(), triggeredPowerQuery.EndDateTime.ParseSolarDateTime())
          .AsReadOnlyList();

        IEnumerable<Task> GetPowerCollectionTasks()
        {
          foreach (var dateRange in dateRanges)
          {
            // (potentially) prepare a new request for a smaller date range than the original request
            var dateRangeRequest = _mapper.Map<PowerQuery, PowerQuery>(triggeredPowerQuery);
            dateRangeRequest.StartDateTime = dateRange.StartDateTime.GetSolarDateTimeString();
            dateRangeRequest.EndDateTime = dateRange.EndDateTime.GetSolarDateTimeString();

            Tracker.TrackInfo($"Sub-orchestrating power refresh for SiteId {triggeredPowerQuery.SiteId} between {dateRangeRequest.StartDateTime} " +
                              $"and {dateRangeRequest.EndDateTime}");

            // sub-orchestrate to limit the date ranges processed
            yield return context.CallSubOrchestratorWithRetryAsync(
              nameof(HydratePowerPeriodSubOrchestrator),
              GetDefaultRetryOptions(exception =>
              {
                var unwrappedException = exception.UnwrapFunctionException();

                if (unwrappedException is SolarEdgeResponseException solarEdgeResponse)
                {
                  // don't bother retrying if unable to get the solar data for these status codes
                  var abortProcessing = solarEdgeResponse.StatusCode == HttpStatusCode.Forbidden ||
                                        solarEdgeResponse.StatusCode == HttpStatusCode.TooManyRequests;

                  Tracker.TrackWarn($"Error while obtaining solar data, aborting = {abortProcessing}", unwrappedException);

                  return !abortProcessing;
                }

                Tracker.TrackWarn("Error while obtaining solar data, will retry", unwrappedException);

                // try again
                return true;
              }),
              dateRangeRequest);
          }
        }

        var tasks = GetPowerCollectionTasks();

        await Task.WhenAll(tasks);

        Tracker.TrackInfo($"All power data refreshed for SiteId {triggeredPowerQuery.SiteId}");

        return await NotifyPowerUpdated(context, PowerUpdatedStatus.Completed, triggeredPowerQuery);
      }
      catch (Exception exception)   // could be SolarEdgeResponseException
      {
        var trackedException = exception.UnwrapFunctionException();

        var notification = new
        {
          triggeredPowerQuery.SiteId,
          ContextInput = triggeredPowerQuery
        };

        Tracker.TrackException(trackedException, notification);

        if (!triggeredPowerQuery.SiteId.IsNullOrEmpty())
        {
          // can't use I/O in an orchestration context so need to indirectly report the problem via another activity
          await context.NotifyException<HydratePowerOrchestrator>(GetDefaultRetryOptions(), triggeredPowerQuery.SiteId, trackedException, notification);
        }

        return await NotifyPowerUpdated(context, PowerUpdatedStatus.Error, triggeredPowerQuery);
      }
    }

    private Task<PowerUpdatedStatus> NotifyPowerUpdated(IDurableOrchestrationContext context, PowerUpdatedStatus status, TriggeredPowerQuery triggeredPowerQuery)
    {
      var message = _mapper.Map<PowerUpdatedMessage>(triggeredPowerQuery);
      message.Status = status;

      Tracker.TrackInfo($"Sending '{status}' status notification for SiteId {message.SiteId}, TriggerType {message.TriggerType}");

      return context.CallActivityWithRetryAsync<PowerUpdatedStatus>(nameof(NotifyPowerUpdatedMessage), GetDefaultRetryOptions(), message);
    }
  }
}