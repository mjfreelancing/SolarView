using AllOverIt.Extensions;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Site
{
  public class SiteEnergyCostsRepository : CloudTableRepository<SiteEnergyCostsEntity>, ISiteEnergyCostsRepository
  {
    public SiteEnergyCostsRepository(CloudTable table)
      : base(table)
    {
    }

    public async Task<IReadOnlyList<SiteEnergyCostsEntity>> GetEnergyCosts(string siteId)
    {
      var energyCosts = await GetAllAsync(siteId);

      return energyCosts.AsReadOnlyList();
    }

    public Task<TableResult> UpsertAsync(SiteEnergyCostsEntity entity)
    {
      return ExecuteAsync(TableOperation.InsertOrMerge, entity);
    }
  }
}