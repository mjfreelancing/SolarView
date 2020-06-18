using System;
using Azure;
using Azure.Security.KeyVault.Secrets;
using SolarViewBlazor.Services.KeyVault.Exceptions;

namespace SolarViewBlazor.Services.KeyVault.Extensions
{
  public static class KeyVaultSecretExtensions
  {
    public static KeyVaultSecret CheckExpiry(this Response<KeyVaultSecret> keyVaultSecret)
    {
      var vaultSecret = keyVaultSecret.Value;
      var expiresOn = vaultSecret.Properties.ExpiresOn;

      if (expiresOn.HasValue)
      {
        var hasExpired = expiresOn.Value.DateTime.ToUniversalTime() < DateTime.UtcNow;

        if (hasExpired)
        {
          throw new SecretExpiredException(vaultSecret);
        }
      }

      return vaultSecret;
    }
  }
}