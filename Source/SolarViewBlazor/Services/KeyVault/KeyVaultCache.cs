using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SolarViewBlazor.Services.KeyVault.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SolarViewBlazor.Services.KeyVault
{
  public class KeyVaultCache : IKeyVaultCache
  {
    private static readonly IDictionary<string, string> SecretsCache = new Dictionary<string, string>();

    private readonly SemaphoreSlim _secretSemaphoreSlim = new SemaphoreSlim(1, 1);

    private readonly IKeyVaultConfiguration _configuration;
    private readonly string _baseUri;
    private static SecretClient _secretClient;

    public KeyVaultCache(IKeyVaultConfiguration configuration)
    {
      _configuration = configuration.WhenNotNull(nameof(configuration));

      _baseUri = $"https://{configuration.KeyVaultId}.vault.azure.net/";
      Reset();
    }

    public string GetSecret(string secretName)
    {
      _secretSemaphoreSlim.Wait();

      try
      {
        return SecretsCache.GetOrSet(secretName, () => _secretClient.GetSecret(secretName).CheckExpiry().Value);
      }
      // todo: handle these
      //catch (RequestFailedException exception)
      //{
      //  // exception.Status == 404 - secret not found
      //  // exception.Status == 403 - secret is disabled
      //}
      finally
      {
        _secretSemaphoreSlim.Release();
      }
    }

    public void Reset()
    {
      _secretSemaphoreSlim.Wait();

      try
      {
        _secretClient = CreateSecretClient();
        SecretsCache.Clear();
      }
      finally
      {
        _secretSemaphoreSlim.Release();
      }
    }

    private SecretClient CreateSecretClient()
    {
      // https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/identity/Azure.Identity
      // https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/keyvault/Azure.Security.KeyVault.Secrets

      var managedCredential = new ManagedIdentityCredential(_configuration.ApplicationClientId);

      // include this if certificate support is ever needed
      //var certificateCredential = new CertificateCredential(_configuration.TenantId, _configuration.ApplicationClientId, certificate);

      var servicePrincipleCredential = new ClientSecretCredential(
        _configuration.TenantId,
        _configuration.ApplicationClientId,
        _configuration.ApplicationClientSecret
      );

      // authenticate using managed identity, otherwise use certificate, otherwise use service principle
      var credential = new ChainedTokenCredential(managedCredential, /*certificateCredential,*/ servicePrincipleCredential);

      return new SecretClient(new Uri(_baseUri), credential);
    }
  }
}