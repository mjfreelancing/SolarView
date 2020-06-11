using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;

namespace SolarViewFunctions.Repository.PowerYearly
{
  public class PowerYearlyRepository : CloudTableRepository<MeterPowerYear>, IPowerYearlyRepository
  {
    public PowerYearlyRepository(CloudTable table)
      : base(table)
    {
    }

    public Task UpsertAsync(IEnumerable<MeterPowerYear> entities)
    {
      return BatchInsertOrReplaceAsync(entities);
    }
  }
}