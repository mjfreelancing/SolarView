using Microsoft.Azure.Cosmos;
using SolarCosmosUtil.Configuration;
using SolarCosmosUtil.Cosmos;
using SolarCosmosUtil.Entities;
using SolarCosmosUtil.KeyVault;
using System.Threading.Tasks;

namespace SolarCosmosUtil.Repository
{
  public class SolarViewCosmosDb : CosmosDbClientBase, ISolarViewCosmosDb
  {
    private const string SolarCosmosEndpoint = "https://solarviewcosmos.documents.azure.com:443/";
    private const string DatabaseId = "solar";
    private const string CollectionId = "energy";
    private const string PartitionKey = "/id";

    public SolarViewCosmosDb(ICosmosConfiguration configuration, IKeyVaultCache keyVaultCache)
      : base(SolarCosmosEndpoint, DatabaseId, CollectionId, PartitionKey, configuration, keyVaultCache)
    {
    }

    public Task<ItemResponse<SolarDocument>> UpsertSolarDocumentAsync(SolarDocument document)
    {
      return Container.UpsertItemAsync(document);
    }
  }
}