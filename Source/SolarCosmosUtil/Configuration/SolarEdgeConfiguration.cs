using AllOverIt.Helpers;
using Microsoft.Extensions.Configuration;

namespace SolarCosmosUtil.Configuration
{
  public class SolarEdgeConfiguration : ConfigurationBase, ISolarEdgeConfiguration
  {
    public string SiteId { get; }
    public string KeyVaultSecretName { get; }

    public SolarEdgeConfiguration(IConfiguration configuration)
      : base(configuration)
    {
      SiteId = Configuration[Constants.SolarEdgeSiteId].WhenNotNullOrEmpty(SiteId);
      KeyVaultSecretName = Configuration[Constants.SolarEdgeSecretName].WhenNotNullOrEmpty(KeyVaultSecretName);
    }
  }
}