using AllOverIt.Extensions;
using AllOverIt.Helpers;
using RestSharp;
using SolarCosmosUtil.Configuration;
using SolarCosmosUtil.Dto.Response;
using SolarCosmosUtil.KeyVault;
using SolarCosmosUtil.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarCosmosUtil.SolarEdge
{
  public class SolarEdgeServiceClient : ISolarEdgeServiceClient
  {
    private static readonly RestClient ApiClient = new RestClient(Constants.SolarEdgeEndpoint);
    private readonly ISolarEdgeConfiguration _configuration;
    private readonly IKeyVaultCache _keyVaultCache;

    public SolarEdgeServiceClient(ISolarEdgeConfiguration configuration, IKeyVaultCache keyVaultCache)
    {
      _configuration = configuration.WhenNotNull(nameof(configuration));
      _keyVaultCache = keyVaultCache.WhenNotNull(nameof(keyVaultCache));
    }

    public async IAsyncEnumerable<SolarViewDay> GetSolarDataAsync(DateTime startDateTime, DateTime endDateTime)
    {
      // todo: add Polly
      // todo: add Application Insight logging on error

      var apiKey = _keyVaultCache.GetSecret(_configuration.KeyVaultSecretName);

      // the API allows a maximum of 1 month per request, so split the dates into monthly requests if required
      var dateRanges = GetDateRanges(startDateTime, endDateTime);

      foreach (var (requestStart, requestEnd) in dateRanges)
      {
        var request = new RestRequest($"site/{_configuration.SiteId}/powerDetails", Method.GET)
          .AddHeader("Accept", "application/json")
          .AddParameter("startTime", $"{requestStart:yyyy-MM-dd HH:mm:ss}")
          .AddParameter("endTime", $"{requestEnd:yyyy-MM-dd HH:mm:ss}")
          .AddParameter("meters", string.Join(',', EnumHelper.GetEnumValues<MeterType>()))
          .AddParameter("api_key", apiKey);

        var response = await ApiClient.ExecuteAsync<SolarData>(request).ConfigureAwait(false);

        if (!response.IsSuccessful)
        {
          // todo: create a custom exception
          throw new Exception(response.ErrorMessage);
        }

        // flattened list of data points - so we can group into days and meter types
        var meterPoints =
          from meter in response.Data.PowerDetails.Meters
          let meterType = meter.Type.As<MeterType>()
          from value in meter.Values
          let timestamp = DateTime.ParseExact(value.Date, "yyyy-MM-dd HH:mm:ss", null)
          let watts = value.Value
          select new
          {
            timestamp.Date,
            MeterType = meterType,
            Timestamp = timestamp,
            Watts = watts
          };

        var solarDays =
          from dailyMeterPoints in meterPoints.GroupBy(item => item.Date)
          select new SolarViewDay
          {
            Date = dailyMeterPoints.Key,
            Meters =
              from dailyMeterPoint in dailyMeterPoints.GroupBy(item => item.MeterType)
              select new SolarViewMeter
              {
                MeterType = dailyMeterPoint.Key,
                Points = dailyMeterPoint
                  .OrderBy(item => item.Timestamp)
                  .Select(item =>
                    new SolarViewMeterPoint
                    {
                      Timestamp = item.Timestamp,
                      Watts = item.Watts
                    })
              }
          };

        foreach (var solarDay in solarDays)
        {
          yield return solarDay;
        }
      }
    }

    private IEnumerable<(DateTime requestStart, DateTime requestEnd)> GetDateRanges(DateTime startDateTime, DateTime endDateTime)
    {
      var startRequestDate = startDateTime;

      do
      {
        var endRequestDate = new DateTime(
          startRequestDate.Year,
          startRequestDate.Month,
          DateTime.DaysInMonth(startRequestDate.Year, startRequestDate.Month),
          23, 59, 59);

        if (endRequestDate > endDateTime)
        {
          endRequestDate = endDateTime;
        }

        yield return (startRequestDate, endRequestDate);

        startRequestDate = endRequestDate.AddDays(1).Date;

      } while (startRequestDate < endDateTime);
    }
  }
}