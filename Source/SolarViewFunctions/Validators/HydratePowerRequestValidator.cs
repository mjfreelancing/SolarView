using SolarViewFunctions.Dto;
using SolarViewFunctions.Validation;

namespace SolarViewFunctions.Validators
{
  public class HydratePowerRequestValidator : ValidatorBase<HydratePowerRequest>
  {
    public HydratePowerRequestValidator()
    {
      RegisterIsRequired(model => model.SiteId);

      // validates both values are provided, in the required format, and represent a valid date range
      RegisterIsValidDateRange(model => model.StartDate, model => model.EndDate, true, "yyyy-MM-dd");
    }
  }
}