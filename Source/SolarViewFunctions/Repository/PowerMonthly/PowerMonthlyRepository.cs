using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;

namespace SolarViewFunctions.Repository.PowerMonthly
{
  public class PowerMonthlyRepository : CloudTableRepository<MeterPowerMonth>, IPowerMonthlyRepository
  {
    public PowerMonthlyRepository(CloudTable table)
      : base(table)
    {
    }

    public Task UpsertAsync(IEnumerable<MeterPowerMonth> entities)
    {
      return BatchInsertOrReplaceAsync(entities);
    }
  }
}