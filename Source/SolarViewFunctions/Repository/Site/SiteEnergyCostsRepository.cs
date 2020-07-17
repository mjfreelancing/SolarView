using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;

namespace SolarViewFunctions.Repository.Site
{
  public class SiteEnergyCostsRepository : CloudTableRepository<SiteEnergyCostsEntity>, ISiteEnergyCostsRepository
  {
    public SiteEnergyCostsRepository(CloudTable table)
      : base(table)
    {
    }

    public Task<SiteEnergyCostsEntity> GetEnergyCosts(string siteId)
    {
      return GetAsync(Constants.Table.SiteEnergyCostsPartitionKey, siteId);
    }

    public Task<TableResult> UpsertAsync(SiteEnergyCostsEntity entity)
    {
      return ExecuteAsync(TableOperation.InsertOrMerge, entity);
    }
  }
}