using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Providers
{
  public interface ISitesUpdateProvider
  {
    // update via a timed function
    Task UpdateSiteAttributeAsync(CloudTable sitesTable, string siteId, IDictionary<string, object> properties);

    // update via a POST request
    Task UpdateSiteEnergyCostsAsync(CloudTable sitesTable, ISiteEnergyCosts energyCosts);
  }
}