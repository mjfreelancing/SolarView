using SolarView.Client.Common.Services.SolarView;

namespace SolarViewBlazor.Configuration
{
  public class SolarViewServiceConfiguration : ISolarViewServiceConfiguration
  {
    public string FunctionsUrl => "https://solarviewfunctions.azurewebsites.net";
    public string KeyVaultFunctionKeyName => Constants.KeyVaultSolarViewFunctionKeyName;
  }
}