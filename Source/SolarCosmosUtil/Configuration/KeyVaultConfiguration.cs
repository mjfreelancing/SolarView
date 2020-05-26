using AllOverIt.Helpers;
using Microsoft.Extensions.Configuration;

namespace SolarCosmosUtil.Configuration
{
  public class KeyVaultConfiguration : ConfigurationBase, IKeyVaultConfiguration
  {
    public string KeyVaultId { get; }
    public string ApplicationClientId { get; }
    public string ApplicationClientSecret { get; }

    public KeyVaultConfiguration(IConfiguration configuration)
      : base(configuration)
    {
      KeyVaultId = Configuration[Constants.KeyVaultId].WhenNotNullOrEmpty(KeyVaultId);
      ApplicationClientId = Configuration[Constants.ApplicationClientId].WhenNotNullOrEmpty(ApplicationClientId);
      ApplicationClientSecret = Configuration[Constants.ApplicationClientSecret].WhenNotNullOrEmpty(ApplicationClientSecret);
    }
  }
}