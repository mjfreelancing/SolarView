using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerUpdateHistory
{
  public class PowerUpdateHistoryRepository : CloudTableRepository<PowerUpdate>, IPowerUpdateHistoryRepository
  {
    public PowerUpdateHistoryRepository(CloudTable table)
      : base(table)
    {
    }

    public Task<IEnumerable<PowerUpdate>> GetPowerUpdatesAsyncEnumerable(string siteId, DateTime date)
    {
      var partitionKey = $"{siteId}_{date.GetSolarDateString()}";

      return GetAllAsync(partitionKey);
    }

    public Task<TableResult> Upsert(PowerUpdate entity)
    {
      return InsertOrReplaceAsync(entity);
    }
  }
}