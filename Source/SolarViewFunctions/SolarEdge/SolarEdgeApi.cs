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
    public static async Task<SolarDataResponse> GetSolarDataAsync(IDurableOrchestrationContext context, string solarEdgeUri, string apiKey,
      PowerQuery powerQuery, ITracker tracker)
    {
      var solarEdgeRequest = CreateSolarEdgeRequest(solarEdgeUri, apiKey, powerQuery);

      tracker.TrackInfo($"Getting solar data for SiteId {powerQuery.SiteId} between {powerQuery.StartDateTime} and {powerQuery.EndDateTime}",
        new {context.InstanceId}
      );

      var response = await context.CallHttpAsync(solarEdgeRequest);

      if (response.StatusCode != HttpStatusCode.OK)
      {
        // 403 - forbidden
        // 429 - too many requests
        tracker.TrackError(
          $"Failed to get solar data for SiteId '{powerQuery.SiteId}'",
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
        
        return SolarDataResponse.Error(response.StatusCode);
      }

      tracker.TrackInfo($"SolarEdge response status = {response.StatusCode}");

      var solarData = JsonConvert.DeserializeObject<SolarData>(response.Content);

      return new SolarDataResponse(solarData);
    }

    private static DurableHttpRequest CreateSolarEdgeRequest(string solarEdgeUri, string apiKey, PowerQuery powerQuery)
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
  }
}