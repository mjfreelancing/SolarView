using AllOverIt.Extensions;
using System;
using System.Globalization;
using System.Linq;

namespace SolarViewFunctions.Validation
{
  public static class StringExtensions
  {
    public static bool IsValidInteger(this string value, bool allowTrailingSign = false)
    {
      if (value.IsNullOrEmpty())
      {
        return false;
      }

      var numberStyle = NumberStyles.Integer;

      if (allowTrailingSign)
      {
        numberStyle |= NumberStyles.AllowTrailingSign;
      }

      return int.TryParse(value, numberStyle, NumberFormatInfo.CurrentInfo, out _);
    }

    public static bool IsValidDouble(this string value, bool allowTrailingSign = false)
    {
      if (value.IsNullOrEmpty())
      {
        return false;
      }

      // Float is a composite of AllowLeadingSign | AllowTrailingWhite | AllowLeadingWhite | AllowExponent | AllowDecimalPoint
      var numberStyle = NumberStyles.Float | NumberStyles.AllowThousands;

      if (allowTrailingSign)
      {
        numberStyle |= NumberStyles.AllowTrailingSign;
      }

      return double.TryParse(value, numberStyle, NumberFormatInfo.CurrentInfo, out _);
    }

    public static bool IsValidBool(this string value)
    {
      return !value.IsNullOrEmpty() && bool.TryParse(value, out _);
    }

    public static bool IsValidDate(this string value, string format)
    {
      return IsValidDate(value, new[] { format });
    }

    public static bool IsValidDate(this string value, string[] formats)
    {
      if (value.IsNullOrEmpty() || formats.IsNullOrEmpty())
      {
        return default;
      }

      return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    public static bool IsValidEnum<TType>(this string value)
      where TType : struct, Enum
    {
      return AllOverIt.Helpers.EnumHelper
        .GetEnumValues<TType>()
        .Any(enumValue => string.Compare($"{enumValue}", value, StringComparison.InvariantCultureIgnoreCase) == 0);
    }

    public static bool IsValidEnum(this string value, Type enumType)
    {
      return enumType.IsEnum &&
             Enum.GetNames(enumType)
               .Any(enumValue => string.Compare(enumValue, value, StringComparison.InvariantCultureIgnoreCase) == 0);
    }
  }
}