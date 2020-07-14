using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Site
{
  public class SiteRepository : CloudTableRepository<SiteEntity>, ISiteRepository
  {
    public SiteRepository(CloudTable table)
      : base(table)
    {
    }

    public Task<SiteEntity> GetSiteAsync(string siteId)
    {
      return GetAsync(Constants.Table.SitesPartitionKey, siteId);
    }

    public IAsyncEnumerable<SiteEntity> GetAllSitesAsyncEnumerable()
    {
      return GetAllAsyncEnumerable(Constants.Table.SitesPartitionKey);
    }

    public Task<TableResult> MergeAsync(ITableEntity entity)
    {
      return ExecuteAsync(TableOperation.InsertOrMerge, entity);
    }
  }
}