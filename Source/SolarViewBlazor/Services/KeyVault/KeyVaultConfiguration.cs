using Microsoft.Extensions.Configuration;

namespace SolarViewBlazor.Services.KeyVault
{
  public class KeyVaultConfiguration : IKeyVaultConfiguration
  {
    public string KeyVaultId { get; }
    public string TenantId { get; }
    public string ApplicationClientId { get; }
    public string ApplicationClientSecret { get; }

    public KeyVaultConfiguration(IConfiguration configuration)
    {
      // All defined in appsettings.json
      KeyVaultId = configuration[Constants.KeyVaultId];
      TenantId = configuration[Constants.TenantId];
      ApplicationClientId = configuration[Constants.ApplicationClientId];

      // Application Setting (portal) and defined in appsettings.Development.json
      ApplicationClientSecret = configuration[Constants.ApplicationClientSecret];
    }
  }
}