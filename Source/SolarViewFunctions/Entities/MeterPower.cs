using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Models;
using System;

namespace SolarViewFunctions.Entities
{
  public class MeterPower : TableEntity
  {
    public string Site { get; set; }
    public string Date { get; set; }
    public string YearMonth { get; set; }
    public string Time { get; set; }
    public string MeterType { get; set; }
    public double Watts { get; set; }

    public MeterPower()
    {
    }

    public MeterPower(string site, DateTime timestamp, MeterType meterType, double watts)
    {
      Site = site;
      Date = $"{timestamp:yyyyMMdd}";
      YearMonth = $"{timestamp:yyyyMM}";
      Time = $"{timestamp:HHmm}";
      MeterType = $"{meterType}";
      Watts = watts;

      PartitionKey = $"{Site}_{Date}_{MeterType}";
      RowKey = $"{Time}";
    }
  }
}