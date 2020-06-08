using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using SolarViewFunctions.Dto;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Tracking;
using SolarViewFunctions.Validators;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerManualHydratePower : FunctionBase
  {
    private readonly IMapper _mapper;

    public TriggerManualHydratePower(ITracker tracker, IMapper mapper)
      : base(tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
    }

    [FunctionName(nameof(TriggerManualHydratePower))]
    public async Task<HttpResponseMessage> Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Power/Hydrate")] HttpRequestMessage request,
      [Table(Constants.Table.Sites)] CloudTable sitesTable,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      var hydrateRequest = await request.Content.ReadAsAsync<HydratePowerRequest>();

      try
      {
        Tracker.TrackEvent(nameof(TriggerManualHydratePower), hydrateRequest);

        ValidateRequest(hydrateRequest);
        await ValidateRequestedSiteAsync(hydrateRequest, sitesTable);

        var powerQuery = _mapper.Map<TriggeredPowerQuery>(hydrateRequest);
        powerQuery.Trigger = RefreshTriggerType.Manual;

        var instanceId = await orchestrationClient.StartNewAsync(nameof(HydratePowerOrchestrator), powerQuery);

        Tracker.TrackInfo(
          $"Started {nameof(HydratePowerOrchestrator)} for a manual power hydration",
          new {InstanceId = instanceId, Request = hydrateRequest}
        );

        // sub task progress / output can be monitored by adding the following to the end of the
        // 'statusQueryGetUri' : &showHistoryOutput=true&showHistory=true
        return orchestrationClient.CreateCheckStatusResponse(request, instanceId);
      }
      catch(PreConditionException exception)
      {
        Tracker.TrackWarn($"Request rejected due to {exception.ErrorCount} precondition failure(s), first = {exception.Errors.First().Message}",
          new
          {
            Request = hydrateRequest,
            Reason = JsonConvert.SerializeObject(exception.Errors, Formatting.None)
          });

        return exception.GetPreConditionErrorResponse();
      }
      catch (Exception exception)
      {
        Tracker.TrackException(exception, new {Request = $"{request}"});

        return exception.GetInternalServerErrorResponse();
      }
    }

    private static void ValidateRequest(HydratePowerRequest request)
    {
      var validator = new HydratePowerRequestValidator();
      var validationResult = validator.Validate(request);

      if (!validationResult.IsValid)
      {
        throw new PreConditionException(validationResult.Errors);
      }
    }

    private static async Task ValidateRequestedSiteAsync(HydratePowerRequest request, CloudTable sitesTable)
    {
      var validator = new HydratePowerSiteValidator(sitesTable);
      var validationResult = await validator.ValidateAsync(request);

      if (!validationResult.IsValid)
      {
        throw new PreConditionException(validationResult.Errors);
      }
    }
  }
}