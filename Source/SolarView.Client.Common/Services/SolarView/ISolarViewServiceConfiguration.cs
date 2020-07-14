namespace SolarView.Client.Common.Services.SolarView
{
  public interface ISolarViewServiceConfiguration
  {
    string FunctionsUrl { get; }
    string KeyVaultFunctionKeyName { get; }
  }
}