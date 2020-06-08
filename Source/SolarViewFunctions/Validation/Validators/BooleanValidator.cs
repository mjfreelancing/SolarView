using FluentValidation.Resources;
using FluentValidation.Validators;

namespace SolarViewFunctions.Validation.Validators
{
  public class BooleanValidator : PropertyValidator
  {
    public BooleanValidator()
      : base(new LanguageStringSource(nameof(BooleanValidator)))
    {
    }

    protected override bool IsValid(PropertyValidatorContext context)
    {
      return context.PropertyValue != null && $"{context.PropertyValue}".IsValidBool();
    }
  }
}