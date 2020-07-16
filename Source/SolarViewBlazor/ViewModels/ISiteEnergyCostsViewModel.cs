using SolarView.Common.Models;
using System.Threading.Tasks;

namespace SolarViewBlazor.ViewModels
{
  public interface ISiteEnergyCostsViewModel
  {
    Task<IEnergyCosts> GetEnergyCosts(string siteId);
  }
}