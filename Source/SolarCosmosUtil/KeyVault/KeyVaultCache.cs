using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Microsoft.Azure.KeyVault;
using Microsoft.Identity.Client;
using SolarCosmosUtil.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SolarCosmosUtil.KeyVault
{
  public class KeyVaultCache : IKeyVaultCache
  {
    private static readonly IDictionary<string, string> SecretsCache = new Dictionary<string, string>();
    private readonly SemaphoreSlim _secretSemaphoreSlim = new SemaphoreSlim(1, 1);
    private static KeyVaultClient _keyVaultClient;
    private readonly IKeyVaultConfiguration _configuration;
    private readonly string _baseUri;

    public KeyVaultCache(IKeyVaultConfiguration configuration)
    {
      _configuration = configuration.WhenNotNull(nameof(configuration));
      _baseUri = $"https://{configuration.KeyVaultId}.vault.azure.net/secrets/";
      Reset();
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
      await _secretSemaphoreSlim.WaitAsync().ConfigureAwait(false);

      try
      {
        var secret = SecretsCache.GetValueOrDefault(secretName);

        if (secret is null)
        {
          var secretBundle = await _keyVaultClient.GetSecretAsync($"{_baseUri}{secretName}").ConfigureAwait(false);

          secret = secretBundle.Value;
          SecretsCache.Add(secretName, secret);
        }

        return secret;
      }
      finally
      {
        _secretSemaphoreSlim.Release();
      }
    }

    // can be called if the key vault secret expires (assuming the configuration is refreshed)
    public void Reset()
    {
      _secretSemaphoreSlim.Wait();

      try
      {
        _keyVaultClient = CreateKeyVaultClient();
        SecretsCache.Clear();
      }
      finally
      {
        _secretSemaphoreSlim.Release();
      }
    }

    private KeyVaultClient CreateKeyVaultClient()
    {
      return new KeyVaultClient(async (authority, resource, scope) =>
      {
        var confidentialClientApplication = ConfidentialClientApplicationBuilder
          .Create(_configuration.ApplicationClientId)
          .WithClientSecret(_configuration.ApplicationClientSecret)
          .WithAuthority(authority)
          .Build();

        var authenticationResult = await confidentialClientApplication
          .AcquireTokenForClient(new[] {"https://vault.azure.net/.default"})
          .ExecuteAsync()
          .ConfigureAwait(false);

        return authenticationResult.AccessToken;
      });
    }
  }
}