namespace SolarCosmosUtil.Configuration
{
  public interface ISolarEdgeConfiguration
  {
    string SiteId { get; }
    string KeyVaultSecretName { get; }
  }
}