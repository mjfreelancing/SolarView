using System.Collections.Generic;
using System.Threading.Tasks;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Repository.Sites;

namespace SolarViewFunctions.Repository.PowerWeekly
{
  public interface IPowerWeeklyRepository : ISolarViewRepository
  {
    Task UpsertAsync(IEnumerable<MeterPowerWeek> entities);
  }
}