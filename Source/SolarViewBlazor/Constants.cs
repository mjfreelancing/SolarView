namespace SolarViewBlazor
{
  internal static class Constants
  {
    // All defined in appsettings.json
    public const string KeyVaultId = "keyVaultId";
    public const string TenantId = "application:tenantId";
    public const string ApplicationClientId = "application:clientId";

    // Application Setting (portal) and defined in appsettings.Development.json
    public const string ApplicationClientSecret = "application:clientSecret";

    // Function auth key to access solarview functions (stored in key vault)
    public const string KeyVaultSolarViewFunctionKeyName = "solarview-function-key";
  }
}