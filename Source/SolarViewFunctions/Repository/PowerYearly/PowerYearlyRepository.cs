using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerYearly
{
  public class PowerYearlyRepository : CloudTableRepository<MeterPowerYearEntity>, IPowerYearlyRepository
  {
    public PowerYearlyRepository(CloudTable table)
      : base(table)
    {
    }

    public IAsyncEnumerable<MeterPowerYearEntity> GetMeterData(string siteId, int year, MeterType meterType)
    {
      var partitionKey = $"{siteId}_{year}_{meterType}";

      return GetAllAsyncEnumerable(partitionKey);
    }

    public Task UpsertYearlyPowerAsync(IEnumerable<MeterPowerYearEntity> entities)
    {
      return BatchInsertOrReplaceAsync(entities);
    }
  }
}