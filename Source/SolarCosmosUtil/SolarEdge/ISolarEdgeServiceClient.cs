using SolarCosmosUtil.Models;
using System;
using System.Collections.Generic;

namespace SolarCosmosUtil.SolarEdge
{
  public interface ISolarEdgeServiceClient
  {
    IAsyncEnumerable<SolarViewDay> GetSolarDataAsync(DateTime startDateTime, DateTime endDateTime);
  }
}