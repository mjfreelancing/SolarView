using AutoMapper;
using SolarView.Client.Common.Models;
using SolarView.Common.Models;

namespace SolarViewBlazor.Mapping
{
  public class SolarViewProfile : Profile
  {
    public SolarViewProfile()
    {
      CreateMap<ISiteEnergyCosts, SiteEnergyCosts>();
    }
  }
}