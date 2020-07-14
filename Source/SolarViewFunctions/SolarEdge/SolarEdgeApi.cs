using AllOverIt.Helpers;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SolarView.Common.Models;
using SolarViewFunctions.Helpers;
using SolarViewFunctions.Models;
using SolarViewFunctions.SolarEdge.Dto.Response;
using SolarViewFunctions.Tracking;
using System.Net;
using System.Threading.Tasks;

namespace SolarViewFunctions.SolarEdge
{
  public class SolarEdgeApi
  {
    public static async Task<PowerDataResult> GetPowerDetailsAsync(IDurableOrchestrationContext context, string solarEdgeUri, string apiKey,
      PowerQuery powerQuery, ITracker tracker)
    {
      var powerDetailsRequest = CreatePowerDetailsRequest(solarEdgeUri, apiKey, powerQuery);

      tracker.TrackInfo($"Getting power data for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}",
        new {context.InstanceId}
      );

      var response = await context.CallHttpAsync(powerDetailsRequest);

      if (response.StatusCode != HttpStatusCode.OK)
      {
        // 403 - forbidden
        // 429 - too many requests
        tracker.TrackError(
          $"Failed to get power data for SiteId '{powerQuery.SiteId}'",
          new
          {
            context.InstanceId,
            context.ParentInstanceId,
            response.StatusCode,
            powerQuery.SiteId,
            StartDate = powerQuery.StartDateTime,
            EndDate = powerQuery.EndDateTime
          }
        );
        
        return PowerDataResult.Error(response.StatusCode);
      }

      tracker.TrackInfo($"SolarEdge response status = {response.StatusCode}");

      var solarData = JsonConvert.DeserializeObject<PowerDataDto>(response.Content);

      return new PowerDataResult(solarData);
    }

    public static async Task<EnergyDataResult> GetEnergyDetailsAsync(IDurableOrchestrationContext context, string solarEdgeUri, string apiKey,
      PowerQuery powerQuery, ITracker tracker)
    {
      var energyDetailsRequest = CreateEnergyDetailsRequest(solarEdgeUri, apiKey, powerQuery);

      tracker.TrackInfo($"Getting energy data for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}",
        new { context.InstanceId }
      );

      var response = await context.CallHttpAsync(energyDetailsRequest);

      if (response.StatusCode != HttpStatusCode.OK)
      {
        // 403 - forbidden
        // 429 - too many requests
        tracker.TrackError(
          $"Failed to get energy data for SiteId '{powerQuery.SiteId}'",
          new
          {
            context.InstanceId,
            context.ParentInstanceId,
            response.StatusCode,
            powerQuery.SiteId,
            StartDate = powerQuery.StartDateTime,
            EndDate = powerQuery.EndDateTime
          }
        );

        return EnergyDataResult.Error(response.StatusCode);
      }

      tracker.TrackInfo($"SolarEdge response status = {response.StatusCode}");

      var solarData = JsonConvert.DeserializeObject<EnergyDataDto>(response.Content);

      return new EnergyDataResult(solarData);
    }

    private static DurableHttpRequest CreatePowerDetailsRequest(string solarEdgeUri, string apiKey, PowerQuery powerQuery)
    {
      // The SolarEdge API treats the end date as exclusive
      return DurableHttpRequestBuilder
        .CreateUri(solarEdgeUri, $"{powerQuery.SiteId}/powerDetails")
        .AddParameter("startTime", powerQuery.StartDateTime)
        .AddParameter("endTime", powerQuery.EndDateTime)
        .AddParameter("meters", string.Join(',', EnumHelper.GetEnumValues<MeterType>()))
        .AddParameter("api_key", apiKey)
        .AddHeader("Accept", "application/json")
        .Build();
    }

    private static DurableHttpRequest CreateEnergyDetailsRequest(string solarEdgeUri, string apiKey, PowerQuery powerQuery)
    {
      // The SolarEdge API treats the end date as exclusive
      return DurableHttpRequestBuilder
        .CreateUri(solarEdgeUri, $"{powerQuery.SiteId}/energyDetails")
        .AddParameter("startTime", powerQuery.StartDateTime)
        .AddParameter("endTime", powerQuery.EndDateTime)
        .AddParameter("meters", string.Join(',', EnumHelper.GetEnumValues<MeterType>()))
        .AddParameter("timeUnit", "QUARTER_OF_AN_HOUR")
        .AddParameter("api_key", apiKey)
        .AddHeader("Accept", "application/json")
        .Build();
    }
  }
}