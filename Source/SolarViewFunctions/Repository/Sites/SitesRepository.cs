using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Sites
{
  public class SitesRepository : CloudTableRepository<SiteInfo>, ISitesRepository
  {
    public SitesRepository(CloudTable table)
      : base(table)
    {
    }

    public Task<SiteInfo> GetSiteAsync(string siteId)
    {
      return GetAsync(Constants.Table.SitesPartitionKey, siteId);
    }

    public IAsyncEnumerable<SiteInfo> GetAllSitesAsyncEnumerable()
    {
      return GetAllAsyncEnumerable(Constants.Table.SitesPartitionKey);
    }

    public Task<TableResult> MergeAsync(ITableEntity entity)
    {
      return ExecuteAsync(TableOperation.InsertOrMerge, entity);
    }
  }
}