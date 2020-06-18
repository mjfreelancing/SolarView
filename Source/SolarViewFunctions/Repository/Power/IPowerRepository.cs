using SolarView.Common.Models;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Repository.Sites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Power
{
  public interface IPowerRepository : ISolarViewRepository
  {
    IAsyncEnumerable<MeterPower> GetMeterPowerAsyncEnumerable(string siteId, DateTime date, MeterType meterType);
    Task UpsertAsync(IEnumerable<MeterPower> entities);
  }
}