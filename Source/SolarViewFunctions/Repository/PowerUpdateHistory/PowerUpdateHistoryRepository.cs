using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerUpdateHistory
{
  public class PowerUpdateHistoryRepository : CloudTableRepository<PowerUpdateEntity>, IPowerUpdateHistoryRepository
  {
    public PowerUpdateHistoryRepository(CloudTable table)
      : base(table)
    {
    }

    public async Task<IEnumerable<PowerUpdateEntity>> GetPowerUpdatesAsyncEnumerable(string siteId, DateTime startDate, DateTime endDate)
    {
      var tasks = new List<Task<IEnumerable<PowerUpdateEntity>>>();

      for (var date = startDate; date <= endDate; date = date.AddDays(1))
      {
        var partitionKey = $"{siteId}_{date.GetSolarDateString()}";

        var task = GetAllAsync(partitionKey);
        tasks.Add(task);
      }

      var updates = await Task.WhenAll(tasks).ConfigureAwait(false);

      return updates.SelectMany(item => item);
    }

    public Task<TableResult> UpsertPowerUpdateAsync(PowerUpdateEntity entity)
    {
      return InsertOrReplaceAsync(entity);
    }
  }
}