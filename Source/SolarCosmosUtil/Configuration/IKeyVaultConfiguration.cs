namespace SolarCosmosUtil.Configuration
{
  // AllOverIt candidate
  public interface IKeyVaultConfiguration
  {
    string TenantId { get; }
    string KeyVaultId { get; }
    string ApplicationClientId { get; }
    string ApplicationClientSecret { get; }
  }
}