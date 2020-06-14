using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Providers
{
  public interface IPowerAggregationProvider
  {
    CloudTable PowerTable { get; set; }
    CloudTable PowerMonthlyTable { get; set; }
    CloudTable PowerYearlyTable { get; set; }

    Task<IEnumerable<TimeWatts>> GetAverageDayView(string siteId, MeterType meterType, DateTime startDate, DateTime endDate);
  }
}