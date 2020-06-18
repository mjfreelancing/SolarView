using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Power
{
  public interface IPowerRepository : ISolarViewRepository
  {
    IAsyncEnumerable<MeterPower> GetMeterPowerAsyncEnumerable(string siteId, DateTime date, MeterType meterType);
    Task UpsertPowerAsync(IEnumerable<MeterPower> entities);
  }
}