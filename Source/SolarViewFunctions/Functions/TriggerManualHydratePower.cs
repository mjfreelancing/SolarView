using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using SolarViewFunctions.Dto;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
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
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public TriggerManualHydratePower(ITracker tracker, IMapper mapper, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(TriggerManualHydratePower))]
    public async Task<HttpResponseMessage> Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Power/Hydrate")] HttpRequestMessage request,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      var triggerDateTime = DateTime.UtcNow;
      var hydrateRequest = await request.Content.ReadAsAsync<HydratePowerRequest>();

      Tracker.AppendDefaultProperties(hydrateRequest);

      try
      {
        Tracker.TrackEvent(nameof(TriggerManualHydratePower), new { TriggerTimeUtc = $"{triggerDateTime.GetSolarDateTimeString()} (UTC)" });

        var siteRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);

        ValidateRequest(hydrateRequest);
        var siteInfo = await GetValidatedRequestedSiteAsync(hydrateRequest, siteRepository);

        var triggeredPowerQuery = _mapper.Map<TriggeredPowerQuery>(hydrateRequest);
        triggeredPowerQuery.TriggerDateTime = siteInfo.UtcToLocalTime(triggerDateTime).GetSolarDateTimeString();
        triggeredPowerQuery.Trigger = RefreshTriggerType.Manual;

        var instanceId = await orchestrationClient.StartNewAsync(nameof(HydratePowerOrchestrator), triggeredPowerQuery);

        Tracker.TrackInfo($"Started {nameof(HydratePowerOrchestrator)} for a manual power hydration of SiteId {siteInfo.SiteId} at " +
                          $"{siteInfo.UtcToLocalTime(triggerDateTime).GetSolarDateTimeString()} (local)");

        // sub task progress / output can be monitored by adding the following to the end of the
        // 'statusQueryGetUri' : &showHistoryOutput=true&showHistory=true
        return orchestrationClient.CreateCheckStatusResponse(request, instanceId);
      }
      catch(PreConditionException exception)
      {
        Tracker.TrackWarn($"Request rejected due to {exception.ErrorCount} precondition failure(s), first = {exception.Errors.First().Message}",
          new {Reason = JsonConvert.SerializeObject(exception.Errors, Formatting.None)});

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

    private static async Task<SiteInfo> GetValidatedRequestedSiteAsync(HydratePowerRequest hydrateRequest, ISitesRepository sitesRepository)
    {
      var siteInfo = await sitesRepository.GetSiteAsync(hydrateRequest.SiteId);

      var validator = new HydratePowerSiteValidator(siteInfo);

      // ReSharper disable once MethodHasAsyncOverload
      var validationResult = validator.Validate(hydrateRequest);

      if (!validationResult.IsValid)
      {
        throw new PreConditionException(validationResult.Errors);
      }

      return siteInfo;
    }
  }
}