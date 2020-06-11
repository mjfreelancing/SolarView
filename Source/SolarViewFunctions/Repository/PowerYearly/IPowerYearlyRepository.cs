using System.Collections.Generic;
using System.Threading.Tasks;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Repository.Sites;

namespace SolarViewFunctions.Repository.PowerYearly
{
  public interface IPowerYearlyRepository : ISolarViewRepository
  {
    Task UpsertAsync(IEnumerable<MeterPowerYear> entities);
  }
}