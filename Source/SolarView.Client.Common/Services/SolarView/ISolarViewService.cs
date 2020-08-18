using SolarView.Client.Common.Models;
using SolarView.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarView.Client.Common.Services.SolarView
{
  // todo: really should split into read/write interfaces
  public interface ISolarViewService
  {
    Task<ISiteDetails> GetSiteDetails(string siteId);
    Task<IReadOnlyList<ISiteEnergyCosts>> GetEnergyCosts(string siteId);
    Task<IReadOnlyList<PowerData>> GetPowerData(string siteId, DateTime startDate, DateTime endDate);
    Task UpsertEnergyCosts(ISiteEnergyCosts energyCosts);
  }
}