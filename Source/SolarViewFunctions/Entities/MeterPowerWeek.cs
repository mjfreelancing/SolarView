using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using System;

namespace SolarViewFunctions.Entities
{
  public class MeterPowerWeek : TableEntity
  {
    public string Site { get; set; }
    public int Year { get; set; }
    public string YearWeek { get; set; }
    public string StartDate { get; set; }       // first date the data covers
    public string EndDate { get; set; }         // last date the data covers
    public string Time { get; set; }
    public int WeekNumber { get; set; }
    public int DayCount { get; set; }           // the first and last week may be partial weeks
    public string MeterType { get; set; }
    public double Watts { get; set; }

    public MeterPowerWeek()
    {
    }

    // startDate/endDate indicates when the data has been aggregated until (for partial week)
    public MeterPowerWeek(string site, DateTime startDate, DateTime endDate, string time, int weekNumber, MeterType meterType, double watts)
    {
      Site = site;
      Year = startDate.Year;
      YearWeek = $"{startDate.Year}{weekNumber:D2}";
      StartDate = startDate.GetSolarDateString();
      EndDate = endDate.GetSolarDateString();
      Time = time;
      WeekNumber = weekNumber;
      DayCount = (endDate - startDate).Days + 1;
      MeterType = $"{meterType}";
      Watts = Math.Round(watts, 6, MidpointRounding.AwayFromZero);

      PartitionKey = $"{Site}_{YearWeek}_{MeterType}";
      RowKey = $"{Time}";
    }
  }
}