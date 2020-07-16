using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;

namespace SolarViewFunctions.Repository.Site
{
  public interface ISiteEnergyCostsRepository : ISolarViewRepository
  {
    Task<SiteEnergyCostsEntity> GetEnergyCosts(string siteId);
    Task<TableResult> MergeAsync(ITableEntity entity);
  }
}