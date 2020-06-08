using FluentValidation;
using SolarViewFunctions.Validation.Validators;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Validation.Extensions
{
  public static class RuleBuilderExtensions
  {
    public static IRuleBuilderOptions<TType, string> IsInteger<TType>(
      this IRuleBuilder<TType, string> ruleBuilder, bool allowTrailingSign)
    {
      return ruleBuilder.SetValidator(new IntegerValidator(allowTrailingSign));
    }

    public static IRuleBuilderOptions<TType, string> IsBoolean<TType>(
      this IRuleBuilder<TType, string> ruleBuilder)
    {
      return ruleBuilder.SetValidator(new BooleanValidator());
    }

    public static IRuleBuilderOptions<TType, string> IsDouble<TType>(
      this IRuleBuilder<TType, string> ruleBuilder, bool allowTrailingSign)
    {
      return ruleBuilder.SetValidator(new DoubleValidator(allowTrailingSign));
    }

    public static IRuleBuilderOptions<TType, TProperty> SatisfiesCriteria<TType, TProperty>(
      this IRuleBuilder<TType, TProperty> ruleBuilder,
      Func<TType, bool> predicate)
    {
      return ruleBuilder.SetValidator(new SatisfiesCriteriaValidator<TType>(predicate));
    }

    public static IRuleBuilderOptions<TType, TProperty> SatisfiesCriteriaAsync<TType, TProperty>(
      this IRuleBuilder<TType, TProperty> ruleBuilder,
      Func<TType, Task<bool>> predicate)
    {
      return ruleBuilder.SetValidator(new SatisfiesCriteriaValidatorAsync<TType>(predicate));
    }

    public static IRuleBuilderOptions<T, string> IsValidEnum<T>(
      this IRuleBuilder<T, string> ruleBuilder, Type enumType)
    {
      return ruleBuilder.SetValidator(new EnumValueValidator(enumType));
    }
  }
}