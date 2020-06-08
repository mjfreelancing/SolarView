namespace SolarViewFunctions.Validation
{
  public interface IValidationError
  {
    string PropertyName { get; }
    object AttemptedValue { get; }
    ValidationReason Reason { get; }
    string Message { get; }
  }
}