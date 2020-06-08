
using FluentValidation.Resources;
using FluentValidation.Validators;
using System;

namespace SolarViewFunctions.Validation.Validators
{
  public class BoundsValidator<TProperty> : PropertyValidator
    where TProperty : struct, IComparable<TProperty>, IComparable
  {
    private readonly IComparable _lowerBound;
    private readonly bool _lowerInclusive;
    private readonly IComparable _upperBound;
    private readonly bool _upperInclusive;

    public BoundsValidator(IComparable lowerBound, bool lowerInclusive, IComparable upperBound, bool upperInclusive)
      : base(new LanguageStringSource(nameof(BoundsValidator<TProperty>)))
    {
      _lowerBound = lowerBound;
      _lowerInclusive = lowerInclusive;
      _upperBound = upperBound;
      _upperInclusive = upperInclusive;
    }

    protected override bool IsValid(PropertyValidatorContext context)
    {
      var propertyValue = (TProperty)context.PropertyValue;

      var lowerIsValid = _lowerInclusive
        ? propertyValue.CompareTo(_lowerBound) >= 0
        : propertyValue.CompareTo(_lowerBound) > 0;

      var upperIsValid = _upperInclusive
        ? propertyValue.CompareTo(_upperBound) <= 0
        : propertyValue.CompareTo(_upperBound) < 0;

      return lowerIsValid && upperIsValid;
    }
  }

  public class BoundsValidator<TProperty, TValue> : PropertyValidator
    where TValue : struct, IComparable<TValue>, IComparable
  {
    private readonly IComparable _lowerBound;
    private readonly bool _lowerInclusive;
    private readonly IComparable _upperBound;
    private readonly bool _upperInclusive;
    private readonly Func<TProperty, TValue> _converter;

    public BoundsValidator(IComparable lowerBound, bool lowerInclusive, IComparable upperBound, bool upperInclusive,
      Func<TProperty, TValue> converter)
      : base(new LanguageStringSource(nameof(BoundsValidator<TProperty, TValue>)))
    {
      _lowerBound = lowerBound;
      _lowerInclusive = lowerInclusive;
      _upperBound = upperBound;
      _upperInclusive = upperInclusive;
      _converter = converter;
    }

    protected override bool IsValid(PropertyValidatorContext context)
    {
      var propertyValue = _converter.Invoke((TProperty) context.PropertyValue);

      var lowerIsValid = _lowerInclusive
        ? propertyValue.CompareTo(_lowerBound) >= 0
        : propertyValue.CompareTo(_lowerBound) > 0;

      var upperIsValid = _upperInclusive
        ? propertyValue.CompareTo(_upperBound) <= 0
        : propertyValue.CompareTo(_upperBound) < 0;

      return lowerIsValid && upperIsValid;
    }
  }
}