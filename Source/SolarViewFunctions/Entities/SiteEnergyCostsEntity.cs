using Microsoft.Azure.Cosmos.Table;
using SolarView.Common.Models;

namespace SolarViewFunctions.Entities
{
  public class SiteEnergyCostsEntity : TableEntity, ISiteEnergyCosts
  {
    // PartitionKey == "EnergyCosts"  (Constants.Table.SiteEnergyCostsPartitionKey)
    // RowKey => value of SiteId
    public string SiteId { get; set; }
    public double CostPerKWhPeak { get; set; }
    public double CostPerKWhOffPeak { get; set; }
    public double SolarBuyBack { get; set; }
  }
}