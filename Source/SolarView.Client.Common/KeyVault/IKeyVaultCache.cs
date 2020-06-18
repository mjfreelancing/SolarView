namespace SolarView.Client.Common.KeyVault
{
  public interface IKeyVaultCache
  {
    string GetSecret(string secretName);
    void Reset();
  }
}