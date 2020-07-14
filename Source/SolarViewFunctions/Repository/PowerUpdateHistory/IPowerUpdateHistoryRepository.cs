using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerUpdateHistory
{
  public interface IPowerUpdateHistoryRepository : ISolarViewRepository
  {
    Task<IEnumerable<PowerUpdateEntity>> GetPowerUpdatesAsyncEnumerable(string siteId, DateTime startDate, DateTime endDate);
    Task<TableResult> UpsertPowerUpdateAsync(PowerUpdateEntity entity);
  }
}