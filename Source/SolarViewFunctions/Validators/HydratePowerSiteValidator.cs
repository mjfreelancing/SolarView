using SolarViewFunctions.Dto;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Validation;

namespace SolarViewFunctions.Validators
{
  public class HydratePowerSiteValidator : ValidatorBase<HydratePowerRequest>
  {
    public HydratePowerSiteValidator(SiteInfo siteInfo)
    {
      RegisterSatisfiesCriteria(model => model.SiteId, model => siteInfo != null, "Site not found");
    }
  }
}