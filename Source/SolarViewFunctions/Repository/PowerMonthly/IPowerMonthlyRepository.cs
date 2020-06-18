using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.PowerMonthly
{
  public interface IPowerMonthlyRepository : ISolarViewRepository
  {
    IAsyncEnumerable<MeterPowerMonth> GetMeterData(string siteId, int year, int month, MeterType meterType);
    Task UpsertMonthlyPowerAsync(IEnumerable<MeterPowerMonth> entities);
  }
}