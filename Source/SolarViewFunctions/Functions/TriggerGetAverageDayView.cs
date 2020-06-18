using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using SolarView.Common.Models;
using SolarViewFunctions.Dto;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.HttpResults;
using SolarViewFunctions.Providers;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Tracking;
using SolarViewFunctions.Validators;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerGetAverageDayView : FunctionBase
  {
    private readonly IPowerAggregationProvider _powerAggregationProvider;
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public TriggerGetAverageDayView(ITracker tracker, IPowerAggregationProvider powerAggregationProvider, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _powerAggregationProvider = powerAggregationProvider.WhenNotNull(nameof(powerAggregationProvider));
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(powerAggregationProvider));
    }

    [FunctionName(nameof(TriggerGetAverageDayView))]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = "power/{siteId}/{meterType}/average")] HttpRequest request, string siteId, string meterType,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [Table(Constants.Table.Power, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerTable,
      [Table(Constants.Table.PowerMonthly, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerMonthlyTable,
      [Table(Constants.Table.PowerYearly, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerYearlyTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      GetAverageDayViewRequest averageDayRequest = null;

      try
      {
        averageDayRequest = new GetAverageDayViewRequest
        {
          SiteId = siteId,
          StartDate = request.Query["StartDate"],
          EndDate = request.Query["EndDate"]
        };

        Tracker.AppendDefaultProperties(averageDayRequest);
        Tracker.TrackEvent(nameof(TriggerGetAverageDayView), new {MeterType = meterType});

        ValidateRequestAsync(averageDayRequest, sitesTable);

        var sitesRepository = _repositoryFactory.Create<ISitesRepository>(sitesTable);
        _ = await averageDayRequest.GetValidatedSiteInfo(sitesRepository);

        _powerAggregationProvider.PowerTable = powerTable;
        _powerAggregationProvider.PowerMonthlyTable = powerMonthlyTable;
        _powerAggregationProvider.PowerYearlyTable = powerYearlyTable;

        var timeWatts = await _powerAggregationProvider.GetAverageDayView(
          averageDayRequest.SiteId,
          meterType.As<MeterType>(),
          averageDayRequest.StartDate.ParseSolarDate(),
          averageDayRequest.EndDate.ParseSolarDate()
        );

        return new OkObjectResult(JsonConvert.SerializeObject(timeWatts));
      }
      catch (PreConditionException exception)
      {
        Tracker.TrackWarn($"Request rejected due to {exception.ErrorCount} precondition failure(s), first = {exception.Errors.First().Message}",
          new { Reason = JsonConvert.SerializeObject(exception.Errors, Formatting.None) });

        return new PreConditionErrorResult(exception);
      }
      catch (Exception exception)
      {
        var notification = new
        {
          averageDayRequest?.SiteId,
          Request = $"{request}",
          RequestContent = averageDayRequest
        };

        Tracker.TrackException(exception, notification);

        if (!averageDayRequest?.SiteId.IsNullOrEmpty() ?? false)
        {
          await exceptionDocuments.AddNotificationAsync<TriggerGetAverageDayView>(averageDayRequest.SiteId, exception, notification).ConfigureAwait(false);
        }

        return new InternalServerErrorResult(exception);
      }
    }

    private static void ValidateRequestAsync(GetAverageDayViewRequest request, CloudTable sitesTable)
    {
      var requestValidator = new GetAverageDayViewRequestValidator();
      var requestValidationResult = requestValidator.Validate(request);

      if (!requestValidationResult.IsValid)
      {
        throw new PreConditionException(requestValidationResult.Errors);
      }
    }
  }
}