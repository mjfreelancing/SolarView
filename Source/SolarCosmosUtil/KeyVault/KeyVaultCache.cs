using AllOverIt.Extensions;
using AllOverIt.Helpers;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SolarCosmosUtil.Configuration;
using SolarCosmosUtil.KeyVault.Extensions;
using SolarCosmosUtil.Logging;
using SolarCosmosUtil.Telemetry;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SolarCosmosUtil.KeyVault
{
  public class KeyVaultCache : IKeyVaultCache
  {
    private static readonly IDictionary<string, string> SecretsCache = new Dictionary<string, string>();

    private readonly SemaphoreSlim _secretSemaphoreSlim = new SemaphoreSlim(1, 1);

    private readonly IKeyVaultConfiguration _configuration;
    private readonly ITelemetryTracker _telemetryTracker;
    private readonly IApplicationLogger _logger;
    private readonly IKeyVaultCachePolicyFactory _keyVaultCachePolicyFactory;
    private readonly string _baseUri;
    private static SecretClient _secretClient;

    // do not inject ITelemetryTracker - it requires this cache (circular dependency)
    // do not inject IApplicationLogger - it is decorated with telemetry tracking
    public KeyVaultCache(IKeyVaultConfiguration configuration/*, ITelemetryTracker telemetryTracker, IApplicationLogger logger*/,
      IKeyVaultCachePolicyFactory keyVaultCachePolicyFactory)
    {
      _configuration = configuration.WhenNotNull(nameof(configuration));
      //_telemetryTracker = telemetryTracker.WhenNotNull(nameof(telemetryTracker));
      //_logger = logger;
      _keyVaultCachePolicyFactory = keyVaultCachePolicyFactory.WhenNotNull(nameof(keyVaultCachePolicyFactory));
      _baseUri = $"https://{configuration.KeyVaultId}.vault.azure.net/";
      Reset();
    }

    public string GetSecret(string secretName)
    {
      // todo: extend this to cater for other RequestFailedException conditions
      // exception.Status == 404 - secret not found
      // exception.Status == 403 - secret is disabled
      var policy = _keyVaultCachePolicyFactory.GetSecretExpiredRetryPolicy(this);

      try
      {
        return policy.Execute(() =>
        {
          _secretSemaphoreSlim.Wait();

          try
          {
            return SecretsCache.GetOrSet(secretName, () => _secretClient.GetSecret(secretName).CheckExpiry().Value);
          }
          finally
          {
            _secretSemaphoreSlim.Release();
          }
        });
      }
      catch (Exception e)
      {
        // todo: track the exception

        throw;
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

      //var certificateCredential = new CertificateCredential(_configuration.TenantId, _configuration.ApplicationClientId, certificate);

      var servicePrincipleCredential = new ClientSecretCredential(
        _configuration.TenantId,
        _configuration.ApplicationClientId,
        _configuration.ApplicationClientSecret
      );

      // todo: add certificate support
      // authenticate using managed identity, otherwise use certificate, otherwise use service principle
      var credential = new ChainedTokenCredential(managedCredential, /*certificateCredential,*/ servicePrincipleCredential);

      return new SecretClient(new Uri(_baseUri), credential);
    }
  }
}