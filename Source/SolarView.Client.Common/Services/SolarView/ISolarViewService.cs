using SolarView.Client.Common.Models;
using SolarView.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarView.Client.Common.Services.SolarView
{
  // used for retrieving aggregated data via the SolarView Functions in Azure
  public interface ISolarViewService
  {
    Task<ISiteDetails> GetSiteDetails(string siteId);
    Task<IEnergyCosts> GetEnergyCosts(string siteId);
    Task<IEnumerable<PowerData>> GetPowerData(string siteId, DateTime startDate, DateTime endDate);
  }
}