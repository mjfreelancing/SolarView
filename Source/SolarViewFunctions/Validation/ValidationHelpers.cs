using AllOverIt.Extensions;
using System;
using System.Globalization;
using System.Linq.Expressions;

namespace SolarViewFunctions.Validation
{
  public static class ValidationHelpers
  {
    // the 'PropertyName' reported in the error is as per the property name but this method ensures
    // when the {PropertyName} is used within WithMessage() it doesn't get split into words.
    public static string GetPropertyName<TType, TProperty>(Expression<Func<TType, TProperty>> expression)
    {
      var property = expression.GetFieldOrProperty();

      return property.Name;
    }

    public static bool TryGetDateValue(string value, string format, out DateTime dateTime)
    {
      return TryGetDateValue(value, new[] { format }, out dateTime);
    }

    public static ValidationError CreatePreConditionError(string propertyName, object attemptedValue, string message)
    {
      return new ValidationError(propertyName, attemptedValue, ValidationReason.InternalPreCondition, message);
    }

    public static ValidationError CreateValidationError(ValidationReason reason, string propertyName, object attemptedValue, string message)
    {
      return new ValidationError(propertyName, attemptedValue, reason, message);
    }

    private static bool TryGetDateValue(string value, string[] formats, out DateTime dateTime)
    {
      dateTime = DateTime.MinValue;

      if (value.IsNullOrEmpty() || formats.IsNullOrEmpty())
      {
        return false;
      }

      return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
    }
  }
}