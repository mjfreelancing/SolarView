using AllOverIt.Extensions;
using FluentValidation.Results;
using System.Linq;

namespace SolarViewFunctions.Validation
{
  public class ValidationError : IValidationError
  {
    public string PropertyName { get; }
    public object AttemptedValue { get; }
    public ValidationReason Reason { get; }
    public string Message { get; }

    public ValidationError(ValidationFailure failure)
    {
      PropertyName = failure.PropertyName;
      AttemptedValue = failure.AttemptedValue;

      if (typeof(ValidationReason).GetEnumNames().Contains(failure.ErrorCode))
      {
        var validationError = failure.ErrorCode.As<ValidationReason>();
        Reason = validationError;
      }
      else
      {
        Reason = ValidationReason.Unknown;
      }

      Message = failure.ErrorMessage;
    }

    public ValidationError(string propertyName, object attemptedValue, ValidationReason reason, string message)
    {
      PropertyName = propertyName;
      AttemptedValue = attemptedValue;
      Reason = reason;
      Message = message;
    }
  }
}