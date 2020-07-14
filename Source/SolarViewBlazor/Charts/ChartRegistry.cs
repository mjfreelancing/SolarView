using SolarView.Client.Common.Models;
using System.Collections.Generic;

namespace SolarViewBlazor.Charts
{
  public class ChartRegistry : IChartRegistry
  {
    private readonly IList<IChartDescriptor> _descriptors = new List<IChartDescriptor>();

    public IEnumerable<IChartDescriptor> ChartDescriptors => _descriptors;

    public void RegisterDescriptor(IChartDescriptor descriptor)
    {
      _descriptors.Add(descriptor);
    }
  }
}