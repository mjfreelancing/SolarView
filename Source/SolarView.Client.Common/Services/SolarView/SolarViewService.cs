﻿using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Flurl;
using Flurl.Http;
using SolarView.Client.Common.KeyVault;
using SolarView.Client.Common.Models;
using SolarView.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SolarView.Client.Common.Services.SolarView
{
  public class SolarViewService : ISolarViewService
  {
    private readonly string _functionsKey;
    private readonly string _functionsUrl;

    public SolarViewService(ISolarViewServiceConfiguration serviceConfiguration, IKeyVaultCache keyVault)
    {
      _functionsKey = keyVault.GetSecret(serviceConfiguration.KeyVaultFunctionKeyName);
      _functionsUrl = serviceConfiguration.FunctionsUrl;
    }

    public async Task<ISiteInfo> GetSiteDetails(string siteId)
    {
      var url = new Url(_functionsUrl)
        .AppendPathSegments("site", siteId)
        .WithHeaders(new
        {
          content_type = "application/json",
          x_functions_key = _functionsKey
        });

      try
      {
        return await url.GetJsonAsync<SiteInfo>();
      }
      catch (FlurlHttpException exception)
      {
        if (exception.Call.HttpStatus.HasValue && exception.Call.HttpStatus == HttpStatusCode.Forbidden)
        {
          // unknown site Id
          return null;
        }

        throw;
      }
    }

    public async Task<IEnumerable<PowerData>> CollectData(string siteId, DateTime startDate, DateTime endDate)
    {
      var requests =
        from meterType in EnumHelper.GetEnumValues<MeterType>()
        select new
        {
          MeterType = meterType,
          Url = new Url(_functionsUrl)
            .AppendPathSegments("site", siteId, "power", meterType, "average")
            .SetQueryParams(new
            {
              StartDate = $"{startDate:yyyy-MM-dd}",
              EndDate = $"{endDate:yyyy-MM-dd}"
            })
            .WithHeaders(new
            {
              content_type = "application/json",
              x_functions_key = _functionsKey
            })
        };

      var requestTasks = requests.Select(request => new
      {
        request.MeterType,
        DataTask = request.Url.GetJsonAsync<IEnumerable<MeterData>>()
      }).AsReadOnlyList();

      var dataTasks = requestTasks.Select(item => item.DataTask);
      await Task.WhenAll(dataTasks).ConfigureAwait(false);

      var allData = requestTasks
        .Select(item => new
        {
          item.MeterType,
          Data = item.DataTask.Result
        });

      var meterData =
        from dataItem in allData
        let meterType = dataItem.MeterType
        from item in dataItem.Data
        select new
        {
          MeterType = meterType,
          item.Time,
          item.Watts,
          item.WattHour
        };

      return meterData
        .GroupBy(item => item.Time)
        .Select(item =>
        {
          var production = item.Single(reading => reading.MeterType == MeterType.Production);
          var consumption = item.Single(reading => reading.MeterType == MeterType.Consumption);
          var feedIn = item.Single(reading => reading.MeterType == MeterType.FeedIn);
          var purchased = item.Single(reading => reading.MeterType == MeterType.Purchased);
          var selfConsumption = item.Single(reading => reading.MeterType == MeterType.SelfConsumption);

          return new PowerData
          {
            Time = item.Key,
            Watts = new WattsData
            {
              Production = production.Watts,
              Consumption = consumption.Watts,
              FeedIn = feedIn.Watts,
              Purchased = purchased.Watts,
              SelfConsumption = selfConsumption.Watts
            },
            WattHour = new WattsData
            {
              Production = production.WattHour,
              Consumption = consumption.WattHour,
              FeedIn = feedIn.WattHour,
              Purchased = purchased.WattHour,
              SelfConsumption = selfConsumption.WattHour
            }
          };
        })
        .AsReadOnlyList();
    }
  }
}
