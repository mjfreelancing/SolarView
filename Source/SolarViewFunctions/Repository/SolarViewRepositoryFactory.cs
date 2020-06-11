using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Repository.Power;
using SolarViewFunctions.Repository.PowerMonthly;
using SolarViewFunctions.Repository.PowerUpdateHistory;
using SolarViewFunctions.Repository.PowerWeekly;
using SolarViewFunctions.Repository.PowerYearly;
using SolarViewFunctions.Repository.Sites;
using System;
using System.Collections.Generic;

namespace SolarViewFunctions.Repository
{
  public class SolarViewRepositoryFactory : ISolarViewRepositoryFactory
  {
    private static readonly IDictionary<string, Func<CloudTable, ISolarViewRepository>> RepositoryRegistry
      = new Dictionary<string, Func<CloudTable, ISolarViewRepository>>
      {
        {Constants.Table.Sites, table => new SitesRepository(table)},
        {Constants.Table.Power, table => new PowerRepository(table)},
        {Constants.Table.PowerWeekly, table => new PowerWeeklyRepository(table)},
        {Constants.Table.PowerMonthly, table => new PowerMonthlyRepository(table)},
        {Constants.Table.PowerYearly, table => new PowerYearlyRepository(table)},
        {Constants.Table.PowerUpdateHistory, table => new PowerUpdateHistoryRepository(table)}
      };

    public TRepository Create<TRepository>(CloudTable table) where TRepository : ISolarViewRepository
    {
      return CreateRepository<TRepository>(table);
    }

    private static TRepository CreateRepository<TRepository>(CloudTable table)
    {
      if (RepositoryRegistry.TryGetValue(table.Name, out var factory))
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