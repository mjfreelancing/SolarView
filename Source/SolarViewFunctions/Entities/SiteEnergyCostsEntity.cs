using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;

namespace SolarViewFunctions.Entities
{
  public class SiteEnergyCostsEntity : TableEntity, IEnergyCosts
  {
    // PartitionKey == "EnergyCosts"  (Constants.Table.SiteEnergyCostsPartitionKey)
    // RowKey => value of SiteId
    public double CostPerKWhPeak { get; set; }
    public double CostPerKWhOffPeak { get; set; }
    public double SolarBuyBack { get; set; }
  }
}