using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using System;

namespace SolarViewFunctions.Entities
{
  public class MeterPowerEntity : TableEntity
  {
    public string Site { get; set; }
    public string Date { get; set; }
    public string YearMonth { get; set; }
    public string Time { get; set; }
    public string MeterType { get; set; }
    public double Watts { get; set; }

    public MeterPowerEntity()
    {
    }

    public MeterPowerEntity(string site, DateTime timestamp, MeterType meterType, double watts)
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