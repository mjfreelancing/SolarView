using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;

namespace SolarViewFunctions.Repository.PowerWeekly
{
  public class PowerWeeklyRepository : CloudTableRepository<MeterPowerWeek>, IPowerWeeklyRepository
  {
    public PowerWeeklyRepository(CloudTable table)
      : base(table)
    {
    }

    public Task UpsertAsync(IEnumerable<MeterPowerWeek> entities)
    {
      return BatchInsertOrReplaceAsync(entities);
    }
  }
}