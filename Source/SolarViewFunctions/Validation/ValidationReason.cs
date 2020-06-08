using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SolarViewFunctions.Validation
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ValidationReason
  {
    Unknown,
    Required,
    ElementRequired,
    InvalidValue,
    DuplicateElement,
    InvalidDate,
    InvalidDateRange,
    OutOfBounds,
    InternalPreCondition,
    InvalidRegex,
    CriteriaFailure
  }
}