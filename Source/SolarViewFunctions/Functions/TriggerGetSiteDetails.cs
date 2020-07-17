using AllOverIt.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SolarViewFunctions.Dto.Response;
using SolarViewFunctions.HttpResults;
using SolarViewFunctions.Repository;
using SolarViewFunctions.Repository.Site;
using SolarViewFunctions.Tracking;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerGetSiteDetails : FunctionBase
  {
    private readonly IMapper _mapper;
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public TriggerGetSiteDetails(ITracker tracker, IMapper mapper, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(TriggerGetSiteDetails))]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = "site/{siteId}")] HttpRequest request, string siteId,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable)
    {
      Tracker.AppendDefaultProperties(new { SiteId = siteId });
      Tracker.TrackEvent(nameof(TriggerGetSiteDetails));

      try
      {
        var sitesRepository = _repositoryFactory.Create<ISiteDetailsRepository>(sitesTable);

        var siteEntity = await sitesRepository.GetSiteAsync(siteId).ConfigureAwait(false);

        if (siteEntity == null)
        {
          return new NotFoundResult();
        }

        var siteInfo = _mapper.Map<SiteDetailsResponse>(siteEntity);

        return new OkObjectResult(siteInfo);
      }
      catch (Exception exception)
      {
        var notification = new
        {
          SiteId = siteId,
          Request = $"{request}"
        };

        Tracker.TrackException(exception, notification);

        return new InternalServerErrorResult(exception);
      }
    }
  }
}