using Microsoft.Azure.Cosmos.Table;

namespace SolarViewFunctions.Repository
{
  public interface ISolarViewRepositoryFactory
  {
    TRepository Create<TRepository>(CloudTable table) where TRepository : ISolarViewRepository;
  }
}