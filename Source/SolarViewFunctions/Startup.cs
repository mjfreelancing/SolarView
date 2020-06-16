using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SolarViewFunctions.Factories;
using SolarViewFunctions.Functions;
using SolarViewFunctions.Providers;
using SolarViewFunctions.Repository;
using SolarViewFunctions.SendGrid;
using SolarViewFunctions.Tracking;

[assembly: FunctionsStartup(typeof(SolarViewFunctions.Startup))]

namespace SolarViewFunctions
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      builder.Services.AddScoped<ITracker, TelemetryTracker>();
      builder.Services.AddScoped<ISendGridEmailCreator, SendGridEmailCreator>();
      builder.Services.AddSingleton<IRetryOptionsFactory, RetryOptionsFactory>();
      builder.Services.AddSingleton<ISolarViewRepositoryFactory, SolarViewRepositoryFactory>();
      builder.Services.AddScoped<IPowerAggregationProvider, PowerAggregationProvider>();
      builder.Services.AddScoped<ISitesUpdateProvider, SitesUpdateProvider>();

      var provider = builder.Services.AddAutoMapper(typeof(Startup)).BuildServiceProvider();
      provider.GetService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();


      // See the link below to see how app settings can be extracted and assigned to a custom type - can be useful for unit testing
      // https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#working-with-options-and-settings


      // using the code below results in the content of host.json being ignore (wrong hub name and route)
      // adding the file to the list makes no difference - now using the ExecutionContext within the functions

      //// https://github.com/Azure/azure-functions-host/issues/4464
      //// Get the path to the folder that has appsettings.json and other files.
      //// Note that there is a better way to get this path: ExecutionContext.FunctionAppDirectory when running inside a function. But we don't have access to the ExecutionContext here.
      //// Functions team should improve this in future. It will hopefully expose FunctionAppDirectory through some other way or env variable.
      //var basePath = IsDevelopmentEnvironment() ?
      //  Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot") :
      //  $"{Environment.GetEnvironmentVariable("HOME")}\\site\\wwwroot";

      //var config = new ConfigurationBuilder()
      //  .SetBasePath(basePath)
      //  //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)  // common settings go here.
      //  //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")}.json", optional: false, reloadOnChange: false)  // environment specific settings go here
      //  .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)  // secrets go here. This file is excluded from source control.
      //  .AddEnvironmentVariables()
      //  .Build();

      //builder.Services.AddSingleton<IConfiguration>(config);
    }

    //private static bool IsDevelopmentEnvironment()
    //{
    //  return "Development".Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase);
    //}
  }
}
