using SolarViewFunctions.Dto;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Validation;

namespace SolarViewFunctions.Validators
{
  public class SiteValidator : ValidatorBase<SiteRequestBase>
  {
    public SiteValidator(SiteInfo siteInfo)
    {
      RegisterSatisfiesCriteria(model => model.SiteId, model => siteInfo != null, "Site not found");
    }
  }
}