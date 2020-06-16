using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerUpdateHistory
{
  public class PowerUpdateHistoryRepository : CloudTableRepository<PowerUpdate>, IPowerUpdateHistoryRepository
  {
    public PowerUpdateHistoryRepository(CloudTable table)
      : base(table)
    {
    }

    public async Task<IEnumerable<PowerUpdate>> GetPowerUpdatesAsyncEnumerable(string siteId, DateTime startDate, DateTime endDate)
    {
      var tasks = new List<Task<IEnumerable<PowerUpdate>>>();

      for (var date = startDate; date <= endDate; date = date.AddDays(1))
      {
        var partitionKey = $"{siteId}_{date.GetSolarDateString()}";

        var task = GetAllAsync(partitionKey);
        tasks.Add(task);
      }

      var updates = await Task.WhenAll(tasks).ConfigureAwait(false);

      return updates.SelectMany(item => item);
    }

    public Task<TableResult> UpsertAsync(PowerUpdate entity)
    {
      return InsertOrReplaceAsync(entity);
    }
  }
}