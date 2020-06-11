using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository.Sites
{
  public interface ISitesRepository : ISolarViewRepository
  {
    Task<SiteInfo> GetSiteAsync(string siteId);
    IAsyncEnumerable<SiteInfo> GetAllSitesAsyncEnumerable();
    Task<TableResult> Upsert(SiteInfo entity);
  }
}