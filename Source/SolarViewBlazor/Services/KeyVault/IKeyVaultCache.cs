namespace SolarViewBlazor.Services.KeyVault
{
  public interface IKeyVaultCache
  {
    string GetSecret(string secretName);
    void Reset();
  }
}