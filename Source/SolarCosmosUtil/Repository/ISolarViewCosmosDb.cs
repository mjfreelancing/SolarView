using Microsoft.Azure.Cosmos;
using SolarCosmosUtil.Entities;
using System.Threading.Tasks;

namespace SolarCosmosUtil.Repository
{
  public interface ISolarViewCosmosDb
  {
    Task<ItemResponse<SolarDocument>> UpsertSolarDocumentAsync(SolarDocument document);
  }
}