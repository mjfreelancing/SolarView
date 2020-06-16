using Microsoft.Azure.Cosmos.Table;
using System.Threading.Tasks;

namespace SolarViewFunctions.Providers
{
  public interface ISitesUpdateProvider
  {
    Task UpdateSiteAttributeAsync(CloudTable sitesTable, string siteId, string propertyName, string value);
  }
}