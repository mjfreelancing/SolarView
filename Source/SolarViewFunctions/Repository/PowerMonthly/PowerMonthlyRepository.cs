using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerMonthly
{
  public class PowerMonthlyRepository : CloudTableRepository<MeterPowerMonthEntity>, IPowerMonthlyRepository
  {
    public PowerMonthlyRepository(CloudTable table)
      : base(table)
    {
    }

    public IAsyncEnumerable<MeterPowerMonthEntity> GetMeterData(string siteId, int year, int month, MeterType meterType)
    {
      var partitionKey = $"{siteId}_{year}{month:D2}_{meterType}";

      return GetAllAsyncEnumerable(partitionKey);
    }

    public Task UpsertMonthlyPowerAsync(IEnumerable<MeterPowerMonthEntity> entities)
    {
      return BatchInsertOrReplaceAsync(entities);
    }
  }
}