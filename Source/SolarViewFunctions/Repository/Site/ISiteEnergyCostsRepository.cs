using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Site
{
  public interface ISiteEnergyCostsRepository : ISolarViewRepository
  {
    Task<IReadOnlyList<SiteEnergyCostsEntity>> GetEnergyCosts(string siteId);
    Task<TableResult> UpsertAsync(SiteEnergyCostsEntity entity);
  }
}