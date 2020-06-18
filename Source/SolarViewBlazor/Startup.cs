using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarViewBlazor.Services;
using SolarViewBlazor.Services.KeyVault;
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
      services.AddRazorPages();
      services.AddServerSideBlazor();
      services.AddSyncfusionBlazor();
      services.AddScoped<ISolarViewService, SolarViewService>();
      services.AddScoped<IKeyVaultConfiguration, KeyVaultConfiguration>();
      services.AddScoped<IKeyVaultCache, KeyVaultCache>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
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

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage("/_Host");
      });
    }
  }
}
