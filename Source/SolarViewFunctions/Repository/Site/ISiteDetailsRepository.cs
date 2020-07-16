using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Site
{
  public interface ISiteDetailsRepository : ISolarViewRepository
  {
    IAsyncEnumerable<SiteDetailsEntity> GetAllSitesAsyncEnumerable();
    Task<SiteDetailsEntity> GetSiteAsync(string siteId);
    Task<TableResult> MergeAsync(ITableEntity entity);
  }
}