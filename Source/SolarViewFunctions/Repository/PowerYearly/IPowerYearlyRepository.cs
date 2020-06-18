using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Repository.Sites;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerYearly
{
  public interface IPowerYearlyRepository : ISolarViewRepository
  {
    IAsyncEnumerable<MeterPowerYear> GetMeterData(string siteId, int year, MeterType meterType);
    Task UpsertAsync(IEnumerable<MeterPowerYear> entities);
  }
}