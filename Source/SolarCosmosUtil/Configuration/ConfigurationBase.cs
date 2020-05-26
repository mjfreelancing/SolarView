using AllOverIt.Helpers;
using Microsoft.Extensions.Configuration;

namespace SolarCosmosUtil.Configuration
{
  public abstract class ConfigurationBase
  {
    protected IConfiguration Configuration { get; }

    protected ConfigurationBase(IConfiguration configuration)
    {
      Configuration = configuration.WhenNotNull(nameof(configuration));
    }
  }
}