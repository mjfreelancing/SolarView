using AllOverIt.Extensions;
using FluentValidation;
using SolarViewFunctions.Dto;
using SolarViewFunctions.Validation;

namespace SolarViewFunctions.Validators
{
  public class GetAverageDayViewRequestValidator : ValidatorBase<GetAverageDayViewRequest>
  {
    public GetAverageDayViewRequestValidator()
    {
      RegisterIsRequired(model => model.SiteId);
      RegisterIsRequired(model => model.StartDate);
      RegisterIsRequired(model => model.EndDate);

      RegisterIsValidDateRange(model => model.StartDate, model => model.EndDate, true, "yyyy-MM-dd")
        .When(model => !model.StartDate.IsNullOrEmpty() && !model.EndDate.IsNullOrEmpty());
    }
  }
}