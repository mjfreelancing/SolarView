using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerYearly
{
  public class PowerYearlyRepository : CloudTableRepository<MeterPowerYear>, IPowerYearlyRepository
  {
    public PowerYearlyRepository(CloudTable table)
      : base(table)
    {
    }

    public IAsyncEnumerable<MeterPowerYear> GetMeterData(string siteId, int year, MeterType meterType)
    {
      var partitionKey = $"{siteId}_{year}_{meterType}";

      return GetAllAsyncEnumerable(partitionKey);
    }

    public Task UpsertAsync(IEnumerable<MeterPowerYear> entities)
    {
      return BatchInsertOrReplaceAsync(entities);
    }
  }
}