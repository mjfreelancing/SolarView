using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using SolarViewFunctions.Dto.Request;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.HttpResults;
using SolarViewFunctions.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Site;
using SolarViewFunctions.Tracking;
using SolarViewFunctions.Validators;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerManualAggregatePower : FunctionBase
  {
    private readonly IMapper _mapper;
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public TriggerManualAggregatePower(ITracker tracker, IMapper mapper, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(TriggerManualAggregatePower))]
    public async Task<HttpResponseMessage> Run(
      [HttpTrigger(AuthorizationLevel.Function, "post", Route = "site/{siteId}/aggregate")]
      HttpRequestMessage request, string siteId,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments,
      [DurableClient] IDurableOrchestrationClient orchestrationClient)
    {
      AggregatePowerRequest aggregateRequest = null;

      try
      {
        aggregateRequest = await request.Content.ReadAsAsync<AggregatePowerRequest>();
        aggregateRequest.SiteId = siteId;

        Tracker.AppendDefaultProperties(aggregateRequest);
        Tracker.TrackEvent(nameof(TriggerManualAggregatePower));

        var siteRepository = _repositoryFactory.Create<ISiteDetailsRepository>(sitesTable);

        ValidateRequest(aggregateRequest);

        var siteInfo = await siteRepository.GetSiteAsync(aggregateRequest.SiteId);

        if (siteInfo == null)
        {
          return new NotFoundResponse();
        }

        var refreshRequest = _mapper.Map<SiteRefreshAggregationRequest>(aggregateRequest);
        refreshRequest.SiteStartDate = siteInfo.StartDate;
        refreshRequest.TriggerType = RefreshTriggerType.Manual;

        // sequentially performs monthly then yearly aggregation
        var instanceId = await orchestrationClient.StartNewAsync(nameof(AggregateSitePowerData), refreshRequest).ConfigureAwait(false);

        Tracker.TrackInfo(
          $"Manual power data aggregation for SiteId {siteId} has been scheduled for {refreshRequest.StartDate} to {refreshRequest.EndDate}",
          new { Request = refreshRequest, InstanceId = instanceId });

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
          SiteId = siteId,
          Request = $"{request}",
          RequestContent = aggregateRequest
        };

        Tracker.TrackException(exception, notification);

        await exceptionDocuments.AddNotificationAsync<TriggerManualHydratePower>(siteId, exception, notification).ConfigureAwait(false);
        await exceptionDocuments.FlushAsync().ConfigureAwait(false);

        return new InternalServerErrorResponse(exception);
      }
    }

    private static void ValidateRequest(SitePeriodRequestBase request)
    {
      var validator = new SitePeriodRequestValidator();
      var validationResult = validator.Validate(request);

      if (!validationResult.IsValid)
      {
        throw new PreConditionException(validationResult.Errors);
      }
    }
  }
}