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
      return GetAsync("SiteId", siteId);
    }

    public IAsyncEnumerable<SiteInfo> GetAllSitesAsyncEnumerable()
    {
      return GetAllAsyncEnumerable("SiteId");
    }

    public Task<TableResult> Upsert(SiteInfo entity)
    {
      return InsertOrReplaceAsync(entity);
    }
  }
}