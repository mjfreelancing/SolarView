namespace SolarViewBlazor.Services.KeyVault
{
  public interface IKeyVaultConfiguration
  {
    string KeyVaultId { get; }
    string TenantId { get; }
    string ApplicationClientId { get; }
    string ApplicationClientSecret { get; }
  }
}