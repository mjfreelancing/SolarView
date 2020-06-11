using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Repository
{
  public class CloudTableRepository<TEntity> : ICloudTableRepository<TEntity>
    where TEntity : ITableEntity, new()
  {
    // https://docs.microsoft.com/en-us/archive/blogs/windowsazurestorage/windows-azure-storage-client-library-2-0-tables-deep-dive
    private readonly CloudTable _table;

    public CloudTableRepository(CloudTable table)
    {
      _table = table.WhenNotNull(nameof(table));
    }

    public TEntity Get(string partitionKey, string rowKey)
    {
      var retrieve = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);

      var tableResult = _table.Execute(retrieve);

      return (TEntity)tableResult.Result;
    }

    public async Task<TEntity> GetAsync(string partitionKey, string rowKey)
    {
      var retrieve = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);

      var tableResult = await _table.ExecuteAsync(retrieve).ConfigureAwait(false);

      return (TEntity)tableResult.Result;
    }

    public Task<IEnumerable<TEntity>> GetAllAsync()
    {
      var tableQuery = new TableQuery<TEntity>();
      return GetAllAsync(tableQuery);
    }

    public IAsyncEnumerable<TEntity> GetAllAsyncEnumerable()
    {
      var tableQuery = new TableQuery<TEntity>();
      return GetAllAsyncEnumerable(tableQuery);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(string partitionKey)
    {
      var tableQuery = new TableQuery<TEntity>()
        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

      return GetAllAsync(tableQuery);
    }

    public IAsyncEnumerable<TEntity> GetAllAsyncEnumerable(string partitionKey)
    {
      var tableQuery = new TableQuery<TEntity>()
        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

      return GetAllAsyncEnumerable(tableQuery);
    }

    public Task<TableResult> InsertAsync(TEntity entity)
    {
      return ExecuteAsync(TableOperation.Insert, entity);
    }

    public Task<TableResult> ReplaceAsync(TEntity entity)
    {
      return ExecuteAsync(TableOperation.Replace, entity);
    }

    public Task<TableResult> MergeAsync(TEntity entity)
    {
      return ExecuteAsync(TableOperation.Merge, entity);
    }

    public Task<TableResult> InsertOrReplaceAsync(TEntity entity)
    {
      return ExecuteAsync(TableOperation.InsertOrReplace, entity);
    }

    public Task<TableResult> InsertOrMergeAsync(TEntity entity)
    {
      return ExecuteAsync(TableOperation.InsertOrMerge, entity);
    }

    public Task<IEnumerable<TableBatchResult>> BatchInsertAsync(IEnumerable<TEntity> entities)
    {
      return DoBatchOperationAsync(entities, (batch, entity) => batch.Insert(entity));
    }

    public IAsyncEnumerable<TableBatchResult> BatchInsertAsyncEnumerable(IEnumerable<TEntity> entities)
    {
      return DoBatchOperationAsyncEnumerable(entities, (batch, entity) => batch.Insert(entity));
    }

    public Task<IEnumerable<TableBatchResult>> BatchInsertOrReplaceAsync(IEnumerable<TEntity> entities)
    {
      return DoBatchOperationAsync(entities, (batch, entity) => batch.InsertOrReplace(entity));
    }

    public IAsyncEnumerable<TableBatchResult> BatchInsertOrReplaceAsyncEnumerable(IEnumerable<TEntity> entities)
    {
      return DoBatchOperationAsyncEnumerable(entities, (batch, entity) => batch.InsertOrReplace(entity));
    }

    public Task<IEnumerable<TableBatchResult>> BatchInsertOrMergeAsync(IEnumerable<TEntity> entities)
    {
      return DoBatchOperationAsync(entities, (batch, entity) => batch.InsertOrMerge(entity));
    }

    public IAsyncEnumerable<TableBatchResult> BatchInsertOrMergeAsyncEnumerable(IEnumerable<TEntity> entities)
    {
      return DoBatchOperationAsyncEnumerable(entities, (batch, entity) => batch.InsertOrMerge(entity));
    }

    public Task<TableResult> DeleteAsync(TEntity entity)
    {
      return ExecuteAsync(TableOperation.Delete, entity);
    }

    private async Task<IEnumerable<TableBatchResult>> DoBatchOperationAsync(IEnumerable<TEntity> entities, Action<TableBatchOperation, ITableEntity> operation)
    {
      IEnumerable<Task<TableBatchResult>> GetBatchTasksAsync()
      {
        foreach (var groupEntities in entities.GroupBy(item => item.PartitionKey))
        {
          var batches = groupEntities.Batch(100);

          foreach (var batch in batches)
          {
            var batchOperation = new TableBatchOperation();

            foreach (var item in batch)
            {
              operation.Invoke(batchOperation, item);
            }

            yield return _table.ExecuteBatchAsync(batchOperation);
          }
        }
      }

      return await Task.WhenAll(GetBatchTasksAsync());
    }

    private async IAsyncEnumerable<TableBatchResult> DoBatchOperationAsyncEnumerable(IEnumerable<TEntity> entities, Action<TableBatchOperation, ITableEntity> operation)
    {
      foreach (var groupEntities in entities.GroupBy(item => item.PartitionKey))
      {
        var batches = groupEntities.Batch(100);

        foreach (var batch in batches)
        {
          var batchOperation = new TableBatchOperation();

          foreach (var item in batch)
          {
            operation.Invoke(batchOperation, item);
          }

          yield return await _table.ExecuteBatchAsync(batchOperation);
        }
      }
    }

    private async Task<IEnumerable<TEntity>> GetAllAsync(TableQuery<TEntity> tableQuery)
    {
      TableContinuationToken token = null;
      var results = new List<TEntity>();

      do
      {
        var queryResult = await _table.ExecuteQuerySegmentedAsync(tableQuery, token).ConfigureAwait(false);

        results.AddRange(queryResult.Results);

        token = queryResult.ContinuationToken;
      } while (token != null);

      return results;
    }

    private async IAsyncEnumerable<TEntity> GetAllAsyncEnumerable(TableQuery<TEntity> tableQuery)
    {
      TableContinuationToken token = null;

      do
      {
        var queryResult = await _table.ExecuteQuerySegmentedAsync(tableQuery, token).ConfigureAwait(false);

        foreach (var result in queryResult.Results)
        {
          yield return result;
        }

        token = queryResult.ContinuationToken;
      } while (token != null);
    }

    private Task<TableResult> ExecuteAsync(Func<ITableEntity, TableOperation> operation, ITableEntity entity)
    {
      return _table.ExecuteAsync(operation.Invoke(entity));
    }
  }
}