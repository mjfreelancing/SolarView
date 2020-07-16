using FluentValidation.Resources;
using FluentValidation.Validators;
using System;

namespace SolarViewFunctions.Validation.Validators
{
  public class SatisfiesCriteriaValidator<TType> : PropertyValidator
  {
    private readonly Func<TType, bool> _predicate;

    public SatisfiesCriteriaValidator(Func<TType, bool> predicate)
      : base(new LanguageStringSource(nameof(SatisfiesCriteriaValidator<TType>)))
    {
      _predicate = predicate;
    }

    protected override bool IsValid(PropertyValidatorContext context)
    {
      return _predicate.Invoke((TType)context.InstanceToValidate);
    }
  }
}