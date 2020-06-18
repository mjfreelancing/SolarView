using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Power
{
  public class PowerRepository : CloudTableRepository<MeterPower>, IPowerRepository
  {
    public PowerRepository(CloudTable table)
      : base(table)
    {
    }

    public IAsyncEnumerable<MeterPower> GetMeterPowerAsyncEnumerable(string siteId, DateTime date, MeterType meterType)
    {
      var partitionKey = $"{siteId}_{date:yyyyMMdd}_{meterType}";

      return GetAllAsyncEnumerable(partitionKey);
    }

    public Task UpsertAsync(IEnumerable<MeterPower> entities)
    {
      return BatchInsertOrReplaceAsync(entities);
    }
  }
}