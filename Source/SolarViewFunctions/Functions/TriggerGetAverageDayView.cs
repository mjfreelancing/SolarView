using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using SolarView.Common.Models;
using SolarViewFunctions.Dto.Request;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.HttpResults;
using SolarViewFunctions.Providers;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Site;
using SolarViewFunctions.Tracking;
using SolarViewFunctions.Validation;
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
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(TriggerGetAverageDayView))]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = "site/{siteId}/power/{meterType}/average")] HttpRequest request, string siteId, string meterType,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [Table(Constants.Table.Power, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerTable,
      [Table(Constants.Table.PowerMonthly, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerMonthlyTable,
      [Table(Constants.Table.PowerYearly, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable powerYearlyTable)
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

        if (!meterType.IsValidEnum<MeterType>())
        {
          return new BadRequestObjectResult($"'{meterType}' is not a valid meter type");
        }

        ValidateRequest(averageDayRequest);

        var siteRepository = _repositoryFactory.Create<ISiteRepository>(sitesTable);

        var siteInfo = await siteRepository.GetSiteAsync(averageDayRequest.SiteId);

        if (siteInfo == null)
        {
          return new ForbiddenResult(null);
        }

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

        return new InternalServerErrorResult(exception);
      }
    }

    //private static bool IsValidMeterType(string meterType)
    //{
    //  if (!meterType.IsValidEnum<MeterType>())
    //  {
    //    var error = ValidationHelpers.CreateValidationError(ValidationReason.InvalidValue, nameof(meterType), meterType, "Invalid Meter Type");
    //    throw new PreConditionException(error);
    //  }
    //}

    private static void ValidateRequest(GetAverageDayViewRequest request)
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