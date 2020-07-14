using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Site;
using System.Threading.Tasks;

namespace SolarViewFunctions.Providers
{
  public class SitesUpdateProvider : ISitesUpdateProvider
  {
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public SitesUpdateProvider(ISolarViewRepositoryFactory repositoryFactory)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    public Task UpdateSiteAttributeAsync(CloudTable sitesTable, string siteId, string propertyName, string value)
    {
      var entity = new DynamicTableEntity(Constants.Table.SitesPartitionKey, siteId);
      entity.Properties.Add(propertyName, new EntityProperty(value));

      var sitesRepository = _repositoryFactory.Create<ISiteRepository>(sitesTable);
      return sitesRepository.MergeAsync(entity);
    }
  }
}