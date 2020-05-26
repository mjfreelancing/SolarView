namespace SolarCosmosUtil.Configuration
{
  // AllOverIt candidate
  public interface IKeyVaultConfiguration
  {
    string KeyVaultId { get; }
    string ApplicationClientId { get; }
    string ApplicationClientSecret { get; }
  }
}