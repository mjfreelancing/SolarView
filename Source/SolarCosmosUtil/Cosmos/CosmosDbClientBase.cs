using AllOverIt.Helpers;
using AllOverIt.Tasks;
using Microsoft.Azure.Cosmos;
using SolarCosmosUtil.Configuration;
using SolarCosmosUtil.KeyVault;

namespace SolarCosmosUtil.Cosmos
{
  public abstract class CosmosDbClientBase
  {
    private readonly string _endpoint;
    private readonly string _databaseId;
    private readonly string _collectionId;
    private readonly string _partitionKey;
    private readonly ICosmosConfiguration _configuration;
    private readonly IKeyVaultCache _keyVaultCache;

    private AsyncLazy<Container> _container;

    protected Container Container => _container.GetAwaiter().GetResult();

    protected CosmosDbClientBase(string endpoint, string databaseId, string collectionId, string partitionKey, ICosmosConfiguration configuration, IKeyVaultCache keyVaultCache)
    {
      _endpoint = endpoint.WhenNotNullOrEmpty(nameof(endpoint));
      _databaseId = databaseId.WhenNotNullOrEmpty(nameof(databaseId));
      _collectionId = collectionId.WhenNotNullOrEmpty(nameof(collectionId));
      _partitionKey = partitionKey.WhenNotNullOrEmpty(nameof(partitionKey));
      _configuration = configuration.WhenNotNull(nameof(configuration));
      _keyVaultCache = keyVaultCache.WhenNotNull(nameof(keyVaultCache));

      Reset();
    }

    private void Reset()
    {
      _keyVaultCache.Reset();

      _container = new AsyncLazy<Container>(async () =>
      {
        var cosmosSecretName = _configuration.KeyVaultSecretName;

        var cosmosKey = _keyVaultCache.GetSecret(cosmosSecretName);
        var cosmosClient = new CosmosClient(_endpoint, cosmosKey);

        var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId).ConfigureAwait(false);
        var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(_collectionId, _partitionKey).ConfigureAwait(false);

        return containerResponse.Container;
      });
    }
  }
}