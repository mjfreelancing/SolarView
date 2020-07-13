using SolarView.Client.Common.Models;
using SolarViewBlazor.Components.Charts;
using System;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts.Descriptors
{
  public class CostBenefitChartDescriptor : IChartDescriptor
  {
    // todo: to be injected
    private const double PurchaseCostPerW = 0.3283379d / 1000.0d;
    private const double FeedInCostPerW = 0.105d / 1000.0d;
    private const double FixedCostPerQuarterHour = 0.99d / 24.0d / 4.0d;

    public string Id => nameof(CostBenefitChartDescriptor);
    public string Description => "Cost Benefit";
    public Type ChartType => typeof(CostBenefitChart);
    public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>
    {
      {nameof(PurchaseCostPerW), PurchaseCostPerW},
      {nameof(FeedInCostPerW), FeedInCostPerW},
      {nameof(FixedCostPerQuarterHour), FixedCostPerQuarterHour}
    };
  }
}