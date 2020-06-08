using AllOverIt.Extensions;
using FluentValidation.Resources;
using FluentValidation.Validators;
using System;
using System.Reflection;

namespace SolarViewFunctions.Validation.Validators
{
  public class EnumValueValidator : PropertyValidator
  {
    private readonly Type _enumType;

    public EnumValueValidator(Type enumType)
      : base(new LanguageStringSource("EnumValueValidator"))
    {
      _enumType = enumType ?? throw new ArgumentNullException(nameof(enumType));

      CheckTypeIsEnum(enumType);
    }

    protected override bool IsValid(PropertyValidatorContext context)
    {
      var value = $"{context.PropertyValue}";

      return value.IsValidInteger()
        ? Enum.IsDefined(_enumType, value.As<int>())
        : value.IsValidEnum(_enumType);
    }

    private static void CheckTypeIsEnum(Type enumType)
    {
      if (!enumType.GetTypeInfo().IsEnum)
      {
        throw new ArgumentOutOfRangeException(nameof(enumType), $"'{enumType.Name}' is not an enum type");
      }
    }
  }
}