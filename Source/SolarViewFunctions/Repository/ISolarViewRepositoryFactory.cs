using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Repository.Sites;

namespace SolarViewFunctions.Repository
{
  public interface ISolarViewRepositoryFactory
  {
    TRepository Create<TRepository>(CloudTable table) where TRepository : ISolarViewRepository;

    //ISitesRepository CreateSitesRepository(CloudTable table);
    //IPowerRepository CreatePowerRepository(CloudTable table);
    //IPowerWeeklyRepository CreatePowerWeeklyRepository(CloudTable table);
    //IPowerMonthlyRepository CreatePowerMonthlyRepository(CloudTable table);
    //IPowerYearlyRepository CreatePowerYearlyRepository(CloudTable table);
    //IPowerUpdateHistoryRepository CreatePowerUpdateRepository(CloudTable table);
  }
}