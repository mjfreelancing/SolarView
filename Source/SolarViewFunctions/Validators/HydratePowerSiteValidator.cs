using System.Threading.Tasks;
using AllOverIt.Helpers;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Dto;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Validation;

namespace SolarViewFunctions.Validators
{
  public class HydratePowerSiteValidator : ValidatorBase<HydratePowerRequest>
  {
    private readonly CloudTable _sitesTable;

    public HydratePowerSiteValidator(CloudTable sitesTable)
    {
      _sitesTable = sitesTable.WhenNotNull(nameof(sitesTable));

      RegisterSatisfiesCriteriaAsync(model => model.SiteId, model => IsValidSite(model.SiteId));
    }

    private async Task<bool> IsValidSite(string siteId)
    {
      var siteInfo = await _sitesTable.GetItemAsync<SiteInfo>("SiteId", siteId).ConfigureAwait(false);

      return siteInfo != null;
    }
  }
}