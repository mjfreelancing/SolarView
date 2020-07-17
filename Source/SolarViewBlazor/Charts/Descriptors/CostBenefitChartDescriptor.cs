using SolarView.Client.Common.Models;
using SolarViewBlazor.Components.Charts;
using System;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts.Descriptors
{
  public class CostBenefitChartDescriptor : IChartDescriptor
  {
    public string Id => nameof(CostBenefitChartDescriptor);
    public string Description => "Cost Benefit";
    public Type ChartType => typeof(CostBenefitChart);
    public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
  }
}