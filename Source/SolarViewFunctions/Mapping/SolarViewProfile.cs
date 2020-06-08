using AutoMapper;
using SolarViewFunctions.Dto;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.Models.Messages;
using System;

namespace SolarViewFunctions.Mapping
{
  public class SolarViewProfile : Profile
  {
    public SolarViewProfile()
    {
      CreateMap<HydratePowerRequest, TriggeredPowerQuery>()
        .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.StartDate))
        .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.EndDate))
        .AfterMap((src, dest) =>
        {
          // force the update to use full day boundaries to ensure docs are not partially replaced
          dest.StartDateTime = DateTime.ParseExact(dest.StartDateTime, "yyyy-MM-dd", null).Date.GetSolarDateTimeString();                         // 00:00:00
          dest.EndDateTime = DateTime.ParseExact(dest.EndDateTime, "yyyy-MM-dd", null).Date.AddDays(1).AddSeconds(-1).GetSolarDateTimeString();   // 23:59:00
        });

      CreateMap<TriggeredPowerQuery, PowerUpdatedMessage>()
        .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDateTime))
        .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDateTime));

      CreateMap<TriggeredPowerQuery, PowerQuery>();

      CreateMap<PowerUpdatedMessage, PowerUpdateEntity>()
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => $"{src.Status}"))
        .AfterMap((src, dest) =>
        {
          dest.PartitionKey = $"{src.SiteId}_{src.StartDate.Substring(0, 10)}";
          dest.RowKey = $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}";
        });
    }
  }
}