using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using SolarViewFunctions.Entities;
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
    private readonly IMapper _mapper;

    public SitesUpdateProvider(ISolarViewRepositoryFactory repositoryFactory, IMapper mapper)
    {
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
      _mapper = mapper.WhenNotNull(nameof(mapper));
    }

    public Task UpdateSiteAttributeAsync(CloudTable sitesTable, string siteId, IDictionary<string, object> properties)
    {
      var entity = new DynamicTableEntity(Constants.Table.SiteDetailsPartitionKey, siteId);

      AddPropertiesToEntity(entity, properties);

      var sitesRepository = _repositoryFactory.Create<ISiteDetailsRepository>(sitesTable);
      return sitesRepository.MergeAsync(entity);
    }

    public Task UpdateSiteEnergyCostsAsync(CloudTable sitesTable, ISiteEnergyCosts energyCosts)
    {
      var entity = _mapper.Map<SiteEnergyCostsEntity>(energyCosts);

      var sitesRepository = _repositoryFactory.Create<ISiteEnergyCostsRepository>(sitesTable);
      return sitesRepository.UpsertAsync(entity);
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