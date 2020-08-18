using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;
using System;

namespace SolarViewFunctions.Entities
{
  public class SiteEnergyCostsEntity : TableEntity, ISiteEnergyCosts
  {
            // PartitionKey == "EnergyCosts"  (Constants.Table.SiteEnergyCostsPartitionKey)
    // PartitionKey => value of SiteId
    // RowKey => value of StartDate
    public string SiteId { get; set; }
    public string StartDate { get; set; }
    public double OffPeakRate { get; set; }
    public double PeakRate { get; set; }
    public double SupplyCharge { get; set; }
    public double SolarBuyBackRate { get; set; }

    public SiteEnergyCostsEntity()
    {
    }

    public SiteEnergyCostsEntity(DateTime startDate)
    {
      StartDate = $"{startDate:yyyy-MM-dd}";
    }
  }
}