using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Providers
{
  public interface ISitesUpdateProvider
  {
    Task UpdateSiteAttributeAsync(CloudTable sitesTable, string siteId, IDictionary<string, object> properties);
    Task UpdateSiteEnergyCostsAsync(CloudTable sitesTable, string siteId, IEnergyCosts energyCosts);
  }
}