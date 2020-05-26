using AllOverIt.Helpers;
using Microsoft.Extensions.Configuration;

namespace SolarCosmosUtil.Configuration
{
  public class CosmosConfiguration : ConfigurationBase, ICosmosConfiguration
  {
    public string KeyVaultSecretName { get; }

    public CosmosConfiguration(IConfiguration configuration)
      : base(configuration)
    {
      KeyVaultSecretName = Configuration[Constants.CosmosSecretName].WhenNotNullOrEmpty(KeyVaultSecretName);
    }
  }
}