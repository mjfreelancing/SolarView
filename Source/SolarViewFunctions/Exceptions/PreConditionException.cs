using AllOverIt.Extensions;
using FluentValidation.Results;
using SolarViewFunctions.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarViewFunctions.Exceptions
{
  public class PreConditionException : Exception
  {
    public override string Message => GetMessage();

    public IEnumerable<ValidationError> Errors { get; }
    public int ErrorCount { get; }

    public PreConditionException(IEnumerable<ValidationFailure> failures)
    {
      var errors = failures.Select(failure => new ValidationError(failure)).AsReadOnlyList();

      Errors = errors;
      ErrorCount = errors.Count;
    }

    private string GetMessage()
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append($"There are {ErrorCount} validation errors");
      stringBuilder.AppendLine();

      foreach (var error in Errors)
      {
        stringBuilder.Append(error.Message); 
      }

      return $"{stringBuilder}";
    }
  }
}