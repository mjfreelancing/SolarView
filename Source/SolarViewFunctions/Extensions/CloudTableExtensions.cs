using AllOverIt.Extensions;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Extensions
{
  public static class CloudTableExtensions
  {
    public static IReadOnlyList<TEntity> GetPartitionItems<TEntity>(this CloudTable table, string partitionKey, Func<TEntity, bool> predicate = null)
      where TEntity : ITableEntity, new()
    {
      var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
      var query = new TableQuery<TEntity>().Where(filter);

      predicate ??= _ => true;

      return table.ExecuteQuery(query).Where(predicate).AsReadOnlyList();
    }

    public static async Task<TEntity> GetItemAsync<TEntity>(this CloudTable table, string partitionKey, string rowKey)
      where TEntity : ITableEntity, new()
    {
      var retrieve = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);

      var result = await table.ExecuteAsync(retrieve);

      return (TEntity)result.Result;
    }

    public static Task InsertOrReplaceAsync<TEntity>(this CloudTable table, TEntity entity)
      where TEntity : ITableEntity
    {
      var operation = TableOperation.InsertOrReplace(entity);
      return table.ExecuteAsync(operation);
    }

    public static Task BatchInsertOrMergeAsync<TEntity>(this CloudTable table, IEnumerable<TEntity> entities)
      where TEntity : ITableEntity
    {
      return DoBatchOperationAsync(table, entities, (batch, entity) => batch.InsertOrMerge(entity));
    }

    private static Task DoBatchOperationAsync<TEntity>(this CloudTable table, IEnumerable<TEntity> entities, Action<TableBatchOperation, TEntity> operation)
      where TEntity : ITableEntity
    {
      IEnumerable<Task> GetBatchTasksAsync()
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

            yield return table.ExecuteBatchAsync(batchOperation);
          }
        }
      }

      return Task.WhenAll(GetBatchTasksAsync());
    }
  }
}