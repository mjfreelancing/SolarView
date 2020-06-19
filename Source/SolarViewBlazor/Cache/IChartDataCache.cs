using SolarViewBlazor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.Cache
{
  public interface IChartDataCache
  {
    Task<int> GetCount();
    Task<IList<ChartData>> GetData();
    Task Add(ChartData chartData);
    Task Remove(string chartId);
  }
}