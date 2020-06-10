using System;
using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;

namespace SolarViewFunctions.Entities
{
  public class MeterPowerYear : TableEntity
  {
    public string Site { get; set; }
    public int Year { get; set; }
    public string StartDate { get; set; }       // first date the data covers
    public string EndDate { get; set; }         // last date the data covers
    public string Time { get; set; }
    public int DayCount { get; set; }           // the first and last month may be partial months
    public string MeterType { get; set; }
    public double Watts { get; set; }

    public MeterPowerYear()
    {
    }

    // startDate/endDate indicates when the data has been aggregated until (for partial month)
    public MeterPowerYear(string site, DateTime startDate, DateTime endDate, string time, MeterType meterType, double watts)
    {
      Site = site;
      Year = startDate.Year;
      StartDate = startDate.GetSolarDateString();
      EndDate = endDate.GetSolarDateString();
      Time = time;
      DayCount = (endDate - startDate).Days + 1;
      MeterType = $"{meterType}";
      Watts = Math.Round(watts, 6, MidpointRounding.AwayFromZero);

      PartitionKey = $"{Site}_{Year}_{MeterType}";
      RowKey = $"{Time}";
    }
  }
}