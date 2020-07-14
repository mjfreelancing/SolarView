using SolarViewFunctions.Dto.Request;
using SolarViewFunctions.Validation;

namespace SolarViewFunctions.Validators
{
  public class SitePeriodRequestValidator : ValidatorBase<SitePeriodRequestBase>
  {
    public SitePeriodRequestValidator()
    {
      // The SiteId is being validated because it is in the route and validated separately

      // validates both values are provided, in the required format, and represent a valid date range
      RegisterIsValidDateRange(model => model.StartDate, model => model.EndDate, true, "yyyy-MM-dd");
    }
  }
}