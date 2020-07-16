using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Site
{
  public class SiteDetailsRepository : CloudTableRepository<SiteDetailsEntity>, ISiteDetailsRepository
  {
    public SiteDetailsRepository(CloudTable table)
      : base(table)
    {
    }

    public IAsyncEnumerable<SiteDetailsEntity> GetAllSitesAsyncEnumerable()
    {
      return GetAllAsyncEnumerable(Constants.Table.SiteDetailsPartitionKey);
    }

    public Task<SiteDetailsEntity> GetSiteAsync(string siteId)
    {
      return GetAsync(Constants.Table.SiteDetailsPartitionKey, siteId);
    }

    public Task<TableResult> MergeAsync(ITableEntity entity)
    {
      return ExecuteAsync(TableOperation.InsertOrMerge, entity);
    }
  }
}