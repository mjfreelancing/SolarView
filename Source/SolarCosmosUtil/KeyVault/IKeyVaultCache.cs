using System.Threading.Tasks;

namespace SolarCosmosUtil.KeyVault
{
  public interface IKeyVaultCache
  {
    void Reset();
    Task<string> GetSecretAsync(string secretName);
  }
}