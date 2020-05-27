using Polly.Retry;

namespace SolarCosmosUtil.KeyVault
{
  public interface IKeyVaultCachePolicyFactory
  {
    RetryPolicy GetSecretExpiredRetryPolicy(IKeyVaultCache keyVaultCache);
  }
}