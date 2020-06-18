using SolarViewBlazor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.Services
{
  public interface ISolarViewService
  {
    Task<IEnumerable<PowerData>> CollectData(string siteId, DateTime startDate, DateTime endDate);
  }
}