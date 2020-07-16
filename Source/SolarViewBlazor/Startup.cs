using AutoMapper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarView.Client.Common.KeyVault;
using SolarView.Client.Common.Services.Site;
using SolarView.Client.Common.Services.SolarView;
using SolarViewBlazor.Cache;
using SolarViewBlazor.Charts;
using SolarViewBlazor.Charts.Descriptors;
using SolarViewBlazor.Charts.ViewModels;
using SolarViewBlazor.Configuration;
using SolarViewBlazor.Events;
using SolarViewBlazor.Services;
using SolarViewBlazor.ViewModels;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace SolarViewBlazor
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;

      // Application Setting (portal) and defined in appsettings.Development.json
      SyncfusionLicenseProvider.RegisterLicense(Configuration["SyncfusionLicense"]);
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services
        .AddAuthentication(AzureADDefaults.AuthenticationScheme)
        .AddAzureAD(options => Configuration.Bind("AzureAd", options));

      services.AddControllersWithViews(options =>
      {
        var policy = new AuthorizationPolicyBuilder()
          .RequireAuthenticatedUser()
          .Build();

        options.Filters.Add(new AuthorizeFilter(policy));
      });

      services.AddRazorPages();
      services.AddServerSideBlazor();
      services.AddSyncfusionBlazor();

      // Keep an eye on this - when out of beta will be worth considering as it uses ASP.NET Data Protection
      // https://www.nuget.org/packages/Microsoft.AspNetCore.ProtectedBrowserStorage
      // https://docs.microsoft.com/en-us/aspnet/core/blazor/state-management
      services.AddBlazoredLocalStorage();

      //services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

      services.AddAutoMapper(typeof(Startup));

      services.AddScoped<ISolarViewService, SolarViewService>();
      services.AddScoped<IKeyVaultConfiguration, KeyVaultConfiguration>();
      services.AddScoped<ISolarViewServiceConfiguration, SolarViewServiceConfiguration>();
      services.AddScoped<IKeyVaultCache, KeyVaultCache>();

      services.AddSingleton<IChartRegistry>(provider =>
      {
        var registry = new ChartRegistry();

        registry.RegisterDescriptor(new ConsumptionChartDescriptor());
        registry.RegisterDescriptor(new CostBenefitChartDescriptor());
        registry.RegisterDescriptor(new FeedInChartDescriptor());

        return registry;
      });

      // there's no state - these only transform / aggregate data for charts
      services.AddSingleton<IConsumptionChartViewModel, ConsumptionChartViewModel>();
      services.AddSingleton<ICostBenefitChartViewModel, CostBenefitChartViewModel>();
      services.AddSingleton<IFeedInChartViewModel, FeedInChartViewModel>();

      services.AddScoped<IChartDataCache, ChartDataCache>();
      services.AddScoped<IEventAggregator, EventAggregator>();
      services.AddScoped<ISiteService, SiteService>();
      services.AddScoped<ISiteViewModel, SiteViewModel>();
      services.AddScoped<ISiteEnergyCostsViewModel, SiteEnergyCostsViewModel>();
      services.AddScoped<ICompareViewModel, CompareViewModel>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMapper mapper)
    {
      mapper.ConfigurationProvider.AssertConfigurationIsValid();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage("/_Host");
      });
    }
  }
}
