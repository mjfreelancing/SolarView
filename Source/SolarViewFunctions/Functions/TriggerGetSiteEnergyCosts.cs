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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerGetSiteEnergyCosts : FunctionBase
  {
    private readonly IMapper _mapper;
    private readonly ISolarViewRepositoryFactory _repositoryFactory;

    public TriggerGetSiteEnergyCosts(ITracker tracker, IMapper mapper, ISolarViewRepositoryFactory repositoryFactory)
      : base(tracker)
    {
      _mapper = mapper.WhenNotNull(nameof(mapper));
      _repositoryFactory = repositoryFactory.WhenNotNull(nameof(repositoryFactory));
    }

    [FunctionName(nameof(TriggerGetSiteEnergyCosts))]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = "site/{siteId}/energyCosts")] HttpRequest request, string siteId,
      [Table(Constants.Table.EnergyCosts, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable energyCostsTable)
    {
      Tracker.AppendDefaultProperties(new { SiteId = siteId });
      Tracker.TrackEvent(nameof(TriggerGetSiteEnergyCosts));

      try
      {
        var sitesRepository = _repositoryFactory.Create<ISiteEnergyCostsRepository>(energyCostsTable);

        var energyCostsEntity = await sitesRepository.GetEnergyCosts(siteId).ConfigureAwait(false);

        if (energyCostsEntity == null)
        {
          return new NotFoundResult();
        }

        var siteInfo = _mapper.Map<IEnumerable<SiteEnergyCostsResponse>>(energyCostsEntity);

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