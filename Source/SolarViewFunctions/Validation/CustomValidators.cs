using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace SolarViewFunctions.Validation
{
  public static class CustomValidators
  {
    public static IRuleBuilderOptions<TType, IEnumerable<TElement>> MustContainUniqueValues<TType, TElement>(this IRuleBuilderInitial<TType, IEnumerable<TElement>> ruleBuilder)
    {
      return ruleBuilder.Must(items => { return items.GroupBy(item => item).All(kvp => kvp.Count() == 1); });
    }

    public static IRuleBuilderOptions<TType, IReadOnlyCollection<TElement>> MustContainFewerThan<TType, TElement>(this IRuleBuilder<TType, IReadOnlyCollection<TElement>> ruleBuilder,
      int maxElements)
    {
      // adds {MaxElements} and {TotalElements} that can be used to format an error message
      return ruleBuilder
        .Must((model, property, context) =>
        {
          context.MessageFormatter
            .AppendArgument("MaxElements", maxElements)
            .AppendArgument("TotalElements", property.Count);

          return property.Count < maxElements;
        });
    }
  }
}