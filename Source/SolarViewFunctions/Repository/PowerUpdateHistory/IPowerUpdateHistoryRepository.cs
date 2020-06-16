using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Repository.Sites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerUpdateHistory
{
  public interface IPowerUpdateHistoryRepository : ISolarViewRepository
  {
    Task<IEnumerable<PowerUpdate>> GetPowerUpdatesAsyncEnumerable(string siteId, DateTime startDate, DateTime endDate);
    Task<TableResult> UpsertAsync(PowerUpdate entity);
  }
}