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
          // Force the update to use full day boundaries to ensure docs are not partially replaced.
          // The SolarEdge API has a granularity of 15 minutes and the end time is exclusive. Any value > 45 will ensure a full day is requested
          dest.StartDateTime = DateTime.ParseExact(dest.StartDateTime, "yyyy-MM-dd", null).Date.GetSolarDateTimeString();                           // 00:00:00
          dest.EndDateTime = DateTime.ParseExact(dest.EndDateTime, "yyyy-MM-dd", null).Date.AddHours(23).AddMinutes(59).GetSolarDateTimeString();   // 23:59:00
        });

      CreateMap<TriggeredPowerQuery, PowerUpdatedMessage>();
      CreateMap<TriggeredPowerQuery, PowerQuery>();

      CreateMap<PowerUpdatedMessage, PowerUpdateEntity>()
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => $"{src.Status}"))
        .AfterMap((src, dest) =>
        {
          // this is currently mapped within a non-durable function so DateTime.UtcNow.Ticks is fine.
          // Even if the mapping was moved to a durable function we're concerned with the RowKey being non-deterministic.
          dest.PartitionKey = $"{src.SiteId}_{src.TriggerDateTime.Substring(0, 10)}";
          dest.RowKey = $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}";
        });
    }
  }
}