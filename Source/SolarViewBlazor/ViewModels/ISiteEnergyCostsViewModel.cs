using SolarView.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public interface ISiteEnergyCostsViewModel
  {
    Task<IReadOnlyList<ISiteEnergyCosts>> GetEnergyCosts(string siteId);
    Task UpdateEnergyCosts(ISiteEnergyCosts energyCosts);
  }
}