using AllOverIt.Extensions;
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
using SolarViewFunctions.HttpResults;
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
      [HttpTrigger(AuthorizationLevel.Function, "post", Route = "hydrate/{siteId}")] HttpRequestMessage request, string siteId,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      HydratePowerRequest hydrateRequest = null;

      try
      {
        var triggerDateTime = DateTime.UtcNow;

        hydrateRequest = await request.Content.ReadAsAsync<HydratePowerRequest>();
        hydrateRequest.SiteId = siteId;

        Tracker.AppendDefaultProperties(hydrateRequest);

        Tracker.TrackEvent(nameof(TriggerManualHydratePower));

        var siteRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);

        ValidateRequest(hydrateRequest);
        var siteInfo = await hydrateRequest.GetValidatedSiteInfo(siteRepository);

        var triggeredPowerQuery = _mapper.Map<TriggeredPowerQuery>(hydrateRequest);
        triggeredPowerQuery.TriggerDateTime = siteInfo.UtcToLocalTime(triggerDateTime).GetSolarDateTimeString();
        triggeredPowerQuery.Trigger = RefreshTriggerType.Manual;

        var instanceId = await orchestrationClient.StartNewAsync(nameof(HydratePowerOrchestrator), triggeredPowerQuery).ConfigureAwait(false);

        Tracker.TrackInfo($"Started {nameof(HydratePowerOrchestrator)} for a manual power hydration of SiteId {siteInfo.SiteId} at " +
                          $"{siteInfo.UtcToLocalTime(triggerDateTime).GetSolarDateTimeString()} (local)");

        // sub task progress / output can be monitored by adding the following to the end of the
        // 'statusQueryGetUri' : &showHistoryOutput=true&showHistory=true
        return orchestrationClient.CreateCheckStatusResponse(request, instanceId);
      }
      catch (PreConditionException exception)
      {
        Tracker.TrackWarn($"Request rejected due to {exception.ErrorCount} precondition failure(s), first = {exception.Errors.First().Message}",
          new { Reason = JsonConvert.SerializeObject(exception.Errors, Formatting.None) });

        return new PreConditionErrorResponse(exception);
      }
      catch (Exception exception)
      {
        var notification = new
        {
          hydrateRequest?.SiteId,
          Request = $"{request}",
          RequestContent = hydrateRequest
        };

        Tracker.TrackException(exception, notification);

        if (!hydrateRequest?.SiteId.IsNullOrEmpty() ?? false)
        {
          await exceptionDocuments.AddNotificationAsync<TriggerManualHydratePower>(hydrateRequest.SiteId, exception, notification).ConfigureAwait(false);
        }

        return new InternalServerErrorResponse(exception);
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
  }
}