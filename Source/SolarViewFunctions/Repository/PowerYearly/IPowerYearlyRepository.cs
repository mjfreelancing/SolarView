using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerYearly
{
  public interface IPowerYearlyRepository : ISolarViewRepository
  {
    IAsyncEnumerable<MeterPowerYear> GetMeterData(string siteId, int year, MeterType meterType);
    Task UpsertYearlyPowerAsync(IEnumerable<MeterPowerYear> entities);
  }
}