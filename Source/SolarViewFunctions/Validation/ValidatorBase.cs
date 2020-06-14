using AllOverIt.Extensions;
using FluentValidation;
using SolarViewFunctions.Validation.Extensions;
using SolarViewFunctions.Validation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SolarViewFunctions.Validation
{
  public abstract class ValidatorBase<TType> : AbstractValidator<TType>
  {
    protected ValidatorBase()
    {
      CascadeMode = CascadeMode.Continue;
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsRequired<TProperty>(Expression<Func<TType, TProperty>> expression)
    {
      return RuleFor(expression)
        .NotEmpty()
        .WithName(ValidationHelpers.GetPropertyName(expression)) // to avoid FluentValidation from splitting the name when more than one word
        .WithMessage("The field '{PropertyName}' is required")
        .WithErrorCode($"{ValidationReason.Required}");
    }

    protected IRuleBuilderOptions<TType, string> RegisterMatchesRegex(Expression<Func<TType, string>> expression, string regex)
    {
      return RuleFor(expression)
        .Matches(regex)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage("The field '{PropertyName}' is not in the correct format.  The value '{PropertyValue}' is invalid.")
        .WithErrorCode($"{ValidationReason.InvalidRegex}");
    }

    // using string fields for enum values
    protected IRuleBuilderOptions<TType, string> RegisterIsValidEnum<TEnum>(Expression<Func<TType, string>> expression)
      where TEnum : Enum
    {
      return RuleFor(expression)
        .IsValidEnum(typeof(TEnum))
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage("The field '{PropertyName}' contains an invalid value '{PropertyValue}'")
        .WithErrorCode($"{ValidationReason.InvalidValue}");
    }

    protected IRuleBuilderOptions<TType, string> RegisterIsValidInteger(Expression<Func<TType, string>> expression, bool allowTrailingSign = false)
    {
      return RuleFor(expression)
        .IsInteger(allowTrailingSign)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage("The field '{PropertyName}' contains an invalid value '{PropertyValue}'")
        .WithErrorCode($"{ValidationReason.InvalidValue}");
    }

    protected IRuleBuilderOptions<TType, string> RegisterIsValidDouble(Expression<Func<TType, string>> expression, bool allowTrailingSign = false)
    {
      return RuleFor(expression)
        .IsDouble(allowTrailingSign)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage("The field '{PropertyName}' contains an invalid value '{PropertyValue}'")
        .WithErrorCode($"{ValidationReason.InvalidValue}");
    }

    protected IRuleBuilderOptions<TType, string> RegisterIsValidBoolean(Expression<Func<TType, string>> expression)
    {
      return RuleFor(expression)
        .IsBoolean()
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage("The field '{PropertyName}' contains an invalid value '{PropertyValue}'")
        .WithErrorCode($"{ValidationReason.InvalidValue}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterNoElementsEmptyOrNull<TProperty>(Expression<Func<TType, IEnumerable<TProperty>>> expression)
    {
      return RuleForEach(expression)
        .NotEmpty()
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage("Element {CollectionIndex} of '{PropertyName}' cannot be empty")
        .WithErrorCode($"{ValidationReason.ElementRequired}");
    }

    protected IRuleBuilderOptions<TType, IEnumerable<TProperty>> RegisterNoDuplicateElements<TProperty>(Expression<Func<TType, IEnumerable<TProperty>>> expression)
    {
      return RuleFor(expression)
        .MustContainUniqueValues()
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage("The field '{PropertyName}' must contain unique values")
        .WithErrorCode($"{ValidationReason.DuplicateElement}");
    }

    protected IRuleBuilderOptions<TType, string> RegisterIsValidDate(Expression<Func<TType, string>> expression, string format)
    {
      return RuleFor(expression)
        .Must(value => StringExtensions.IsValidDate(value, format))
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage("The field '{PropertyName}' is not in the required date format. The value '{PropertyValue}' is invalid.")
        .WithErrorCode($"{ValidationReason.InvalidDate}");
    }

    // using string fields for date values
    protected IRuleBuilderOptions<TType, string> RegisterIsValidDateRange(Expression<Func<TType, string>> expression1, Expression<Func<TType, string>> expression2,
      bool allowSameDate, string format)
    {
      return RegisterDateRangeValidation(
        expression1,
        expression2,
        allowSameDate,
        expression => RegisterIsValidDate(expression, format),
        value => ValidationHelpers.GetDateValue(value, format)
      );
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsGreaterThan<TProperty>(Expression<Func<TType, TProperty>> expression, TProperty value)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .GreaterThan(value)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be greater than {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    // Example usage that ensures a property is a string and that string, when converted to an integer, is greater than zero
    // RegisterIsValidInteger(model => model.Year)
    //   .DependentRules(() =>
    //   {
    //     RegisterIsGreaterThan(model => model.Year, value => value.As<int>(), 0);
    //   });
    protected IRuleBuilderOptions<TType, TProperty> RegisterIsGreaterThan<TProperty, TAsType>(Expression<Func<TType, TProperty>> expression, Func<TProperty, TAsType> converter,
      TAsType value)
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .Must(model => converter.Invoke(model).CompareTo(value) > 0)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be greater than {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsGreaterThan<TProperty>(Expression<Func<TType, TProperty?>> expression, TProperty value)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .GreaterThan(value)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be greater than {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsGreaterThan<TProperty, TAsType>(Expression<Func<TType, TProperty?>> expression, Func<TProperty?, TAsType> converter,
      TAsType value)
      where TProperty : struct, IComparable<TProperty>, IComparable
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .Must(model => converter.Invoke(model).CompareTo(value) > 0)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be greater than {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsGreaterThanOrEqualTo<TProperty>(Expression<Func<TType, TProperty>> expression, TProperty value)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .GreaterThanOrEqualTo(value)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be greater than or equal to {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsGreaterThanOrEqualTo<TProperty, TAsType>(Expression<Func<TType, TProperty>> expression, Func<TProperty, TAsType> converter,
      TAsType value)
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .Must(model => converter.Invoke(model).CompareTo(value) >= 0)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be greater than or equal to {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsGreaterThanOrEqualTo<TProperty>(Expression<Func<TType, TProperty?>> expression, TProperty value)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .GreaterThanOrEqualTo(value)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be greater than or equal to {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsGreaterThanOrEqualTo<TProperty, TAsType>(Expression<Func<TType, TProperty?>> expression, Func<TProperty?, TAsType> converter,
      TAsType value)
      where TProperty : struct, IComparable<TProperty>, IComparable
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .Must(model => converter.Invoke(model).CompareTo(value) >= 0)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be greater than or equal to {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsLessThan<TProperty>(Expression<Func<TType, TProperty>> expression, TProperty value)
     where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .LessThan(value)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be less than {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsLessThan<TProperty, TAsType>(Expression<Func<TType, TProperty>> expression, Func<TProperty, TAsType> converter,
      TAsType value)
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .Must(model => converter.Invoke(model).CompareTo(value) < 0)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be less than {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsLessThan<TProperty>(Expression<Func<TType, TProperty?>> expression, TProperty value)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .LessThan(value)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be less than {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsLessThan<TProperty, TAsType>(Expression<Func<TType, TProperty?>> expression, Func<TProperty?, TAsType> converter,
      TAsType value)
      where TProperty : struct, IComparable<TProperty>, IComparable
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .Must(model => converter.Invoke(model).CompareTo(value) < 0)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be less than {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsLessThanOrEqualTo<TProperty>(Expression<Func<TType, TProperty>> expression, TProperty value)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .LessThanOrEqualTo(value)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be less than or equal to {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsLessThanOrEqualTo<TProperty, TAsType>(Expression<Func<TType, TProperty>> expression, Func<TProperty, TAsType> converter,
      TAsType value)
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .Must(model => converter.Invoke(model).CompareTo(value) <= 0)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be less than or equal to {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsLessThanOrEqualTo<TProperty>(Expression<Func<TType, TProperty?>> expression, TProperty value)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .LessThanOrEqualTo(value)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be less than or equal to {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsLessThanOrEqualTo<TProperty, TAsType>(Expression<Func<TType, TProperty?>> expression, Func<TProperty?, TAsType> converter,
      TAsType value)
      where TProperty : struct, IComparable<TProperty>, IComparable
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .Must(model => converter.Invoke(model).CompareTo(value) <= 0)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' must be less than or equal to {value}")
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsAllowedValue<TProperty>(Expression<Func<TType, TProperty>> expression,
      IEnumerable<TProperty> allowedValues)
    {
      return RuleFor(expression)
      .Must(allowedValues.Contains)
      .WithName(ValidationHelpers.GetPropertyName(expression))
      .WithMessage("The field '{PropertyName}' contains an invalid value '{PropertyValue}'")
      .WithErrorCode($"{ValidationReason.InvalidValue}");
    }

    protected IRuleBuilderOptions<TType, string> RegisterSatisfiesCriteria(Expression<Func<TType, string>> expression, Func<TType, bool> predicate,
      string criteriaMessage)
    {
      return RuleFor(expression)
        .SatisfiesCriteria(predicate)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' has an invalid value '{{PropertyValue}}': {criteriaMessage}")
        .WithErrorCode($"{ValidationReason.CriteriaFailure}");
    }

    protected IRuleBuilderOptions<TType, string> RegisterSatisfiesCriteriaAsync(Expression<Func<TType, string>> expression, Func<TType, Task<bool>> predicate,
      string criteriaMessage)
    {
      return RuleFor(expression)
        .SatisfiesCriteriaAsync(predicate)
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage($"The field '{{PropertyName}}' has an invalid value '{{PropertyValue}}': {criteriaMessage}")
        .WithErrorCode($"{ValidationReason.CriteriaFailure}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsWithinBounds<TProperty>(Expression<Func<TType, TProperty>> expression,
      TProperty lowerBound, bool lowerInclusive, TProperty upperBound, bool upperInclusive)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .SetValidator(new BoundsValidator<TProperty>(lowerBound, lowerInclusive, upperBound, upperInclusive))
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage(GetBoundsErrorMessage(lowerBound, lowerInclusive, upperBound, upperInclusive))
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty> RegisterIsWithinBounds<TProperty, TAsType>(Expression<Func<TType, TProperty>> expression,
      Func<TProperty, TAsType> converter,
      TAsType lowerBound, bool lowerInclusive, TAsType upperBound, bool upperInclusive)
      where TAsType : struct, IComparable<TAsType>, IComparable
    {
      return RuleFor(expression)
        .SetValidator(new BoundsValidator<TProperty, TAsType>(lowerBound, lowerInclusive, upperBound, upperInclusive, converter))
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage(GetBoundsErrorMessage(lowerBound, lowerInclusive, upperBound, upperInclusive))
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    protected IRuleBuilderOptions<TType, TProperty?> RegisterIsWithinBounds<TProperty>(Expression<Func<TType, TProperty?>> expression,
      TProperty? lowerBound, bool lowerInclusive, TProperty? upperBound, bool upperInclusive)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      return RuleFor(expression)
        .SetValidator(new BoundsValidator<TProperty>(lowerBound, lowerInclusive, upperBound, upperInclusive))
        .WithName(ValidationHelpers.GetPropertyName(expression))
        .WithMessage(GetBoundsErrorMessage(lowerBound, lowerInclusive, upperBound, upperInclusive))
        .WithErrorCode($"{ValidationReason.OutOfBounds}");
    }

    private static string GetBoundsErrorMessage<TProperty>(TProperty lowerBound, bool lowerInclusive, TProperty upperBound, bool upperInclusive)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      var messagePrefix = "The field '{PropertyName}' is not within the bounds ";
      var lowerComparison = GetComparison(lowerInclusive);
      var upperComparison = GetComparison(upperInclusive);
      var bounds = $"{lowerBound} {lowerComparison} value {upperComparison} {upperBound}";

      return $"{messagePrefix}{bounds}";
    }

    private static string GetBoundsErrorMessage<TProperty>(TProperty? lowerBound, bool lowerInclusive, TProperty? upperBound, bool upperInclusive)
      where TProperty : struct, IComparable<TProperty>, IComparable
    {
      var messagePrefix = "The field '{PropertyName}' is not within the bounds ";
      var lowerComparison = GetComparison(lowerInclusive);
      var upperComparison = GetComparison(upperInclusive);
      var bounds = $"{lowerBound} {lowerComparison} value {upperComparison} {upperBound}";

      return $"{messagePrefix}{bounds}";
    }

    private IRuleBuilderOptions<TType, string> RegisterDateRangeValidation(Expression<Func<TType, string>> expression1, Expression<Func<TType, string>> expression2,
      bool allowSameDate, Action<Expression<Func<TType, string>>> registerIsValidDate, Func<string, DateTime?> getDateValue)
    {
      //RegisterIsRequired(expression1).DependentRules(() => registerIsValidDate.Invoke(expression1));
      //RegisterIsRequired(expression2).DependentRules(() => registerIsValidDate.Invoke(expression2));

      return RuleFor(expression1)
        .Must((model, property, context) =>
        {
          var otherPropertyName = ValidationHelpers.GetPropertyName(expression2);
          var otherPropertyValue = expression2.Compile().Invoke(model);

          context.MessageFormatter
            .AppendArgument("ComparedPropertyName", otherPropertyName)
            .AppendArgument("ComparedPropertyValue", otherPropertyValue);

          var date1 = getDateValue.Invoke(property);
          var date2 = getDateValue.Invoke(otherPropertyValue);

          return allowSameDate
            ? date1 <= date2
            : date1 < date2;
        })
        .When(model =>
        {
          var value1 = expression1.Compile().Invoke(model);
          var date1 = getDateValue.Invoke(value1);

          var value2 = expression2.Compile().Invoke(model);
          var date2 = getDateValue.Invoke(value2);

          return date1 != null && date2 != null;
        })
        .WithName(ValidationHelpers.GetPropertyName(expression1))
        .WithMessage(allowSameDate
          ? "The field '{PropertyName}' must have a date less than or equal to '{ComparedPropertyName}'"
          : "The field '{PropertyName}' must have a date less than '{ComparedPropertyName}'")
        .WithErrorCode($"{ValidationReason.InvalidDateRange}");
    }

    private static string GetComparison(bool inclusive)
    {
      return inclusive ? "<=" : "<";
    }
  }
}