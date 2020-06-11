using System.Collections.Generic;
using System.Threading.Tasks;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Repository.Sites;

namespace SolarViewFunctions.Repository.PowerMonthly
{
  public interface IPowerMonthlyRepository : ISolarViewRepository
  {
    Task UpsertAsync(IEnumerable<MeterPowerMonth> entities);
  }
}