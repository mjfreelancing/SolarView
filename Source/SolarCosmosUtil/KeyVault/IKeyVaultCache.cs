namespace SolarCosmosUtil.KeyVault
{
  public interface IKeyVaultCache
  {
    string GetSecret(string secretName);
    void Reset();
  }
}