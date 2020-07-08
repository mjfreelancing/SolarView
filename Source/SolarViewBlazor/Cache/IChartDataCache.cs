using SolarViewBlazor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.Cache
{
  public interface IChartDataCache
  {
    Task<int> GetCount(string siteId);
    Task<IReadOnlyList<ChartData>> GetData(string siteId);
    Task Add(string siteId, ChartData chartData);
    Task Remove(string siteId, string chartId);
  }
}