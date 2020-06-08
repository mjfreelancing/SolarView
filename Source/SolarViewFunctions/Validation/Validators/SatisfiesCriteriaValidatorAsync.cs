using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Resources;
using FluentValidation.Validators;

namespace SolarViewFunctions.Validation.Validators
{
  public class SatisfiesCriteriaValidatorAsync<TType> : AsyncValidatorBase
  {
    private readonly Func<TType, Task<bool>> _predicateAsync;

    public SatisfiesCriteriaValidatorAsync(Func<TType, Task<bool>> predicate)
      : base(new LanguageStringSource(nameof(SatisfiesCriteriaValidatorAsync<TType>)))
    {
      _predicateAsync = predicate;
    }

    protected override Task<bool> IsValidAsync(PropertyValidatorContext context, CancellationToken cancellation)
    {
      return _predicateAsync.Invoke((TType)context.Instance);
    }
  }
}