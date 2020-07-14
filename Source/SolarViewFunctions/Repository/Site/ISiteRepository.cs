using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Site
{
  public interface ISiteRepository : ISolarViewRepository
  {
    Task<SiteEntity> GetSiteAsync(string siteId);
    IAsyncEnumerable<SiteEntity> GetAllSitesAsyncEnumerable();
    Task<TableResult> MergeAsync(ITableEntity entity);
  }
}