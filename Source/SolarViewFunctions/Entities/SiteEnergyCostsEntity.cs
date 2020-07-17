using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;

namespace SolarViewFunctions.Entities
{
  public class SiteEnergyCostsEntity : TableEntity, ISiteEnergyCosts
  {
    // PartitionKey == "EnergyCosts"  (Constants.Table.SiteEnergyCostsPartitionKey)
    // RowKey => value of SiteId
    public string SiteId { get; set; }
    public double OffPeakRate { get; set; }
    public double PeakRate { get; set; }
    public double SupplyCharge { get; set; }
    public double SolarBuyBackRate { get; set; }
  }
}