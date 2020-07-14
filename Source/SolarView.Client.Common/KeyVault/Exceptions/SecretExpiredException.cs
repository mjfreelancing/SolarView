using AllOverIt.Helpers;
using Azure.Security.KeyVault.Secrets;
using System;

namespace SolarView.Client.Common.KeyVault.Exceptions
{
  public class SecretExpiredException : Exception
  {
    public KeyVaultSecret VaultSecret { get; }

    public SecretExpiredException(KeyVaultSecret vaultSecret)
      : base(GetErrorMessage(vaultSecret))
    {
      VaultSecret = vaultSecret.WhenNotNull(nameof(vaultSecret));
    }

    private static string GetErrorMessage(KeyVaultSecret vaultSecret)
    {
      // there should be an expiry, but let's not assume
      return vaultSecret.Properties.ExpiresOn.HasValue 
        ? $"The secret '{vaultSecret.Name}' expired at {vaultSecret.Properties.ExpiresOn.Value.DateTime.ToUniversalTime():yyyy-MM-dd HH:mm:ss} (UTC)"
        : $"The secret '{vaultSecret.Name}' has expired";
    }
  }
}