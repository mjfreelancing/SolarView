using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Flurl;
using Flurl.Http;
using SolarViewBlazor.Models;
using SolarViewBlazor.Services.KeyVault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewBlazor.Services
{
  public class SolarViewService : ISolarViewService
  {
    private string _url = "https://solarviewfunctions.azurewebsites.net";
    private readonly string _functionsKey;

    public SolarViewService(IKeyVaultCache keyVault)
    {
      _functionsKey = keyVault.GetSecret(Constants.KeyVaultSolarViewFunctionKeyName);
    }

    public async Task<IEnumerable<PowerData>> CollectData(string siteId, DateTime startDate, DateTime endDate)
    {
      var requests =
        from meterType in EnumHelper.GetEnumValues<MeterType>()
        select new
        {
          MeterType = meterType,
          Url = new Url(_url)
            .AppendPathSegments("power", siteId, meterType, "average")
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

      var allDataTasks = requests.Select(request => new
      {
        request.MeterType,
        DataTask = request.Url.GetJsonAsync<IEnumerable<MeterData>>()
      }).AsReadOnlyList();

      await Task.WhenAll(allDataTasks.Select(item => item.DataTask)).ConfigureAwait(false);

      var allData = allDataTasks.Select(item => new{item.MeterType, Data = item.DataTask.Result});

      var meterData =
        from dataItem in allData
        let meterType = dataItem.MeterType
        from item in dataItem.Data
        select new
        {
          MeterType = meterType,
          item.Time,
          item.Watts
        };

      return meterData
        .GroupBy(item => item.Time)
        .Select(item =>
        {
          return new PowerData
          {
            Time = item.Key,
            Production = item.Single(reading => reading.MeterType == MeterType.Production).Watts,
            Consumption = item.Single(reading => reading.MeterType == MeterType.Consumption).Watts,
            FeedIn = item.Single(reading => reading.MeterType == MeterType.FeedIn).Watts,
            Purchased = item.Single(reading => reading.MeterType == MeterType.Purchased).Watts,
            SelfConsumption = item.Single(reading => reading.MeterType == MeterType.SelfConsumption).Watts
          };
        })
        .AsReadOnlyList();
    }
  }
}
