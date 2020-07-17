using SolarView.Common.Models;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public interface ISiteEnergyCostsViewModel
  {
    Task<ISiteEnergyCosts> GetEnergyCosts(string siteId);
    Task UpdateEnergyCosts(ISiteEnergyCosts energyCosts);
  }
}