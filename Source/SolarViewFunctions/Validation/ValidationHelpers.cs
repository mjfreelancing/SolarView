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

      return property.Name ?? string.Empty;
    }

    public static DateTime? GetDateValue(string value, string format)
    {
      return GetDateValue(value, new[] { format });
    }

    public static ValidationError CreatePreConditionError(string propertyName, object attemptedValue, string message)
    {
      return new ValidationError(propertyName, attemptedValue, ValidationReason.InternalPreCondition, message);
    }

    private static DateTime? GetDateValue(string value, string[] formats)
    {
      if (value.IsNullOrEmpty() || formats.IsNullOrEmpty())
      {
        return default;
      }

      return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime)
        ? dateTime
        : default(DateTime?);
    }
  }
}