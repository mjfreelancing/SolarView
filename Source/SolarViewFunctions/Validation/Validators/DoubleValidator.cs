using FluentValidation.Resources;
using FluentValidation.Validators;

namespace SolarViewFunctions.Validation.Validators
{
  public class DoubleValidator : PropertyValidator
  {
    private readonly bool _allowTrailingSign;

    public DoubleValidator(bool allowTrailingSign)
      : base(new LanguageStringSource(nameof(DoubleValidator)))
    {
      _allowTrailingSign = allowTrailingSign;
    }

    protected override bool IsValid(PropertyValidatorContext context)
    {
      return context.PropertyValue != null && $"{context.PropertyValue}".IsValidDouble(_allowTrailingSign);
    }
  }
}