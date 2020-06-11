using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository
{
  public interface ICloudTableRepository<TEntity>
    where TEntity : ITableEntity, new()
  {
    TEntity Get(string partitionKey, string rowKey);
    Task<TEntity> GetAsync(string partitionKey, string rowKey);

    Task<IEnumerable<TEntity>> GetAllAsync();
    IAsyncEnumerable<TEntity> GetAllAsyncEnumerable();

    Task<IEnumerable<TEntity>> GetAllAsync(string partitionKey);
    IAsyncEnumerable<TEntity> GetAllAsyncEnumerable(string partitionKey);

    Task<TableResult> InsertAsync(TEntity entity);
    Task<TableResult> ReplaceAsync(TEntity entity);
    Task<TableResult> MergeAsync(TEntity entity);

    Task<TableResult> InsertOrReplaceAsync(TEntity entity);
    Task<TableResult> InsertOrMergeAsync(TEntity entity);

    Task<IEnumerable<TableBatchResult>> BatchInsertAsync(IEnumerable<TEntity> entities);
    IAsyncEnumerable<TableBatchResult> BatchInsertAsyncEnumerable(IEnumerable<TEntity> entities);

    Task<IEnumerable<TableBatchResult>> BatchInsertOrReplaceAsync(IEnumerable<TEntity> entities);
    IAsyncEnumerable<TableBatchResult> BatchInsertOrReplaceAsyncEnumerable(IEnumerable<TEntity> entities);

    Task<IEnumerable<TableBatchResult>> BatchInsertOrMergeAsync(IEnumerable<TEntity> entities);
    IAsyncEnumerable<TableBatchResult> BatchInsertOrMergeAsyncEnumerable(IEnumerable<TEntity> entities);

    Task<TableResult> DeleteAsync(TEntity entity);
  }
}