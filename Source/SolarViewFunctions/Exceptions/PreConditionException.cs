using AllOverIt.Extensions;
using FluentValidation.Results;
using SolarViewFunctions.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolarViewFunctions.Exceptions
{
  public class PreConditionException : Exception
  {
    public IEnumerable<ValidationError> Errors { get; }
    public int ErrorCount { get; }

    public PreConditionException(IEnumerable<ValidationFailure> failures)
    {
      var errors = failures.Select(failure => new ValidationError(failure)).AsReadOnlyList();

      Errors = errors;
      ErrorCount = errors.Count;
    }
  }
}