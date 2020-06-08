using FluentValidation.Resources;
using FluentValidation.Validators;

namespace SolarViewFunctions.Validation.Validators
{
  public class IntegerValidator : PropertyValidator
  {
    private readonly bool _allowTrailingSign;

    public IntegerValidator(bool allowTrailingSign)
      : base(new LanguageStringSource(nameof(IntegerValidator)))
    {
      _allowTrailingSign = allowTrailingSign;
    }

    protected override bool IsValid(PropertyValidatorContext context)
    {
      return context.PropertyValue != null && $"{context.PropertyValue}".IsValidInteger(_allowTrailingSign);
    }
  }
}