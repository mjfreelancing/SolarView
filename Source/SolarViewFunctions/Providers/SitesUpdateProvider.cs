using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Site;
using System;
using System.Collections.Generic;
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

    public Task UpdateSiteAttributeAsync(CloudTable sitesTable, string siteId, IDictionary<string, object> properties)
    {
      var entity = new DynamicTableEntity(Constants.Table.SiteDetailsPartitionKey, siteId);

      AddPropertiesToEntity(entity, properties);

      var sitesRepository = _repositoryFactory.Create<ISiteDetailsRepository>(sitesTable);
      return sitesRepository.MergeAsync(entity);
    }

    public Task UpdateSiteEnergyCostsAsync(CloudTable sitesTable, string siteId, IEnergyCosts energyCosts)
    {
      var entity = new DynamicTableEntity(Constants.Table.SiteEnergyCostsPartitionKey, siteId);

      var properties = energyCosts.ToPropertyDictionary();
      AddPropertiesToEntity(entity, properties);

      var sitesRepository = _repositoryFactory.Create<ISiteEnergyCostsRepository>(sitesTable);
      return sitesRepository.MergeAsync(entity);
    }

    private static void AddPropertiesToEntity(DynamicTableEntity entity, IDictionary<string, object> properties)
    {
      foreach (var (propertyName, value) in properties)
      {
        entity.Properties.Add(propertyName, CreateEntityProperty(value));
      }
    }

    private static EntityProperty CreateEntityProperty(object value)
    {
      return value switch
      {
        string strValue => new EntityProperty(strValue),
        double doubleValue => new EntityProperty(doubleValue),
        _ => throw new InvalidOperationException($"Entity property type '{value.GetType().Name}' not supported")
      };
    }
  }
}