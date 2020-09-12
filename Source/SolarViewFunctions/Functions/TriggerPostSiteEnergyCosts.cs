using AllOverIt.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SolarViewFunctions.Dto.Request;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.HttpResults;
using SolarViewFunctions.Providers;
using SolarViewFunctions.Tracking;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class TriggerPostSiteEnergyCosts : FunctionBase
  {
    private readonly ISitesUpdateProvider _sitesUpdateProvider;

    public TriggerPostSiteEnergyCosts(ITracker tracker, ISitesUpdateProvider sitesUpdateProvider)
      : base(tracker)
    {
      _sitesUpdateProvider = sitesUpdateProvider.WhenNotNull(nameof(sitesUpdateProvider));
    }

    [FunctionName(nameof(TriggerPostSiteEnergyCosts))]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "post", Route = "site/{siteId}/energyCosts")] HttpRequestMessage request, string siteId,
      [Table(Constants.Table.Sites, Connection = Constants.ConnectionStringNames.SolarViewStorage)] CloudTable sitesTable,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.ExceptionCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<ExceptionDocument> exceptionDocuments)
    {
      SiteEnergyCostsPostRequest energyCostsRequest = null;

      try
      {
        energyCostsRequest = await request.Content.ReadAsAsync<SiteEnergyCostsPostRequest>();
        energyCostsRequest.SiteId = siteId;

        Tracker.AppendDefaultProperties(new { SiteId = siteId });
        Tracker.TrackEvent(nameof(TriggerPostSiteEnergyCosts));

        await _sitesUpdateProvider.UpdateSiteEnergyCostsAsync(sitesTable, energyCostsRequest);

        return new OkResult();
      }
      catch (Exception exception)
      {
        var notification = new
        {
          SiteId = siteId,
          Request = $"{request}",
          RequestContent = energyCostsRequest
        };

        Tracker.TrackException(exception, notification);

        await exceptionDocuments.AddNotificationAsync<TriggerPostSiteEnergyCosts>(siteId, exception, notification).ConfigureAwait(false);
        await exceptionDocuments.FlushAsync().ConfigureAwait(false);

        return new InternalServerErrorResult(exception);
      }
    }
  }
}