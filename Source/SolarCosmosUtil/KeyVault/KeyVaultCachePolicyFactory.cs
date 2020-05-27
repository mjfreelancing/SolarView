using Polly;
using Polly.Retry;
using SolarCosmosUtil.KeyVault.Exceptions;

namespace SolarCosmosUtil.KeyVault
{
  public class KeyVaultCachePolicyFactory : IKeyVaultCachePolicyFactory
  {
    public RetryPolicy GetSecretExpiredRetryPolicy(IKeyVaultCache keyVaultCache)
    {
     return Policy
       .Handle<SecretExpiredException>()
       .Retry(1, (exception, retryCount, context) =>
       {
         // todo: track / log the exception

         keyVaultCache.Reset();
       });
    }
  }
}