using AllOverIt.Extensions;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Repository.Power;
using SolarViewFunctions.Repository.PowerMonthly;
using SolarViewFunctions.Repository.PowerUpdateHistory;
using SolarViewFunctions.Repository.PowerYearly;
using SolarViewFunctions.Repository.Site;
using System;
using System.Collections.Generic;

namespace SolarViewFunctions.Repository
{
  public class SolarViewRepositoryFactory : ISolarViewRepositoryFactory
  {
    private class RepositoryKey
    {
      public static readonly RepositoryKeyEqualityComparer Comparer = new RepositoryKeyEqualityComparer();

      public string TableName { get; }
      public Type InterfaceType { get; }

      public RepositoryKey(string tableName, Type interfaceType)
      {
        TableName = tableName;
        InterfaceType = interfaceType;
      }
    }

    private sealed class RepositoryKeyEqualityComparer : IEqualityComparer<RepositoryKey>
    {
      public bool Equals(RepositoryKey lhs, RepositoryKey rhs)
      {
        if (lhs == null && rhs == null)
        {
          return true;
        }

        if (lhs == null || rhs == null)
        {
          return false;
        }

        return lhs.TableName == rhs.TableName &&
               lhs.InterfaceType == rhs.InterfaceType;
      }

      public int GetHashCode(RepositoryKey obj)
      {
        return obj.CalculateHashCode();
      }
    }

    private static readonly IDictionary<RepositoryKey, Func<CloudTable, ISolarViewRepository>> RepositoryRegistry
      = new Dictionary<RepositoryKey, Func<CloudTable, ISolarViewRepository>>(RepositoryKey.Comparer)
      {
        {new RepositoryKey(Constants.Table.Sites, typeof(ISiteDetailsRepository)), table => new SiteDetailsRepository(table)},
        {new RepositoryKey(Constants.Table.Sites, typeof(ISiteEnergyCostsRepository)), table => new SiteEnergyCostsRepository(table)},
        {new RepositoryKey(Constants.Table.Power, typeof(IPowerRepository)), table => new PowerRepository(table)},
        {new RepositoryKey(Constants.Table.PowerMonthly, typeof(IPowerMonthlyRepository)), table => new PowerMonthlyRepository(table)},
        {new RepositoryKey(Constants.Table.PowerYearly, typeof(IPowerYearlyRepository)), table => new PowerYearlyRepository(table)},
        {new RepositoryKey(Constants.Table.PowerUpdateHistory, typeof(IPowerUpdateHistoryRepository)), table => new PowerUpdateHistoryRepository(table)}
      };

    public TRepository Create<TRepository>(CloudTable table) where TRepository : ISolarViewRepository
    {
      return CreateRepository<TRepository>(table);
    }

    private static TRepository CreateRepository<TRepository>(CloudTable table)
    {
      var repositoryKey = new RepositoryKey(table.Name, typeof(TRepository));

      if (RepositoryRegistry.TryGetValue(repositoryKey, out var factory))
      {
        var repository = factory.Invoke(table);

        if (repository is TRepository repo)
        {
          return repo;
        }

        throw new ArgumentException($"The Table '{table.Name}' cannot be used to create a repository of type '{typeof(TRepository)}'", nameof(table));
      }

      throw new ArgumentException($"Table '{table.Name}' not registered", nameof(table));
    }
  }
}