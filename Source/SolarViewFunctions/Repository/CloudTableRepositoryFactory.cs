using Microsoft.Azure.Cosmos.Table;

namespace SolarViewFunctions.Repository
{
  // a general purpose static factory for cases where you only want access to the methods offered by ICloudTableRepository
  public static class CloudTableRepository
  {
    public static ICloudTableRepository<TEntity> CreateRepository<TEntity>(CloudTable table)
      where TEntity : ITableEntity, new()
    {
      return new CloudTableRepository<TEntity>(table);
    }
  }
}