using AllOverIt.Helpers;
using Microsoft.Extensions.Configuration;

namespace SolarCosmosUtil.Configuration
{
  public class ApplicationInsightsConfiguration : ConfigurationBase, IApplicationInsightsConfiguration
  {
    public string KeyVaultSecretName { get; }

    public ApplicationInsightsConfiguration(IConfiguration configuration)
      : base(configuration)
    {
      KeyVaultSecretName = Configuration[Constants.ApplicationInsightsSecretName].WhenNotNullOrEmpty(KeyVaultSecretName);
    }
  }
}