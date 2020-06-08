using AllOverIt.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using SolarViewFunctions.Models;
using SolarViewFunctions.SolarEdge.Dto.Response;
using SolarViewFunctions.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarViewFunctions.Functions
{
  public class PersistSolarDataAsPowerDocuments : FunctionBase
  {
    public PersistSolarDataAsPowerDocuments(ITracker tracker)
      : base(tracker)
    {
    }

    [FunctionName(nameof(PersistSolarDataAsPowerDocuments))]
    public async Task Run(
      [ActivityTrigger] IDurableActivityContext context,
      [CosmosDB(Constants.Cosmos.SolarDatabase, Constants.Cosmos.PowerCollection,
        ConnectionStringSetting = Constants.ConnectionStringNames.SolarViewCosmos)] IAsyncCollector<PowerDocument> powerDocuments)
    {
      // allowing exceptions to bubble back to the caller

      var siteData = context.GetInput<SiteSolarData>();

      Tracker.TrackInfo(
        $"Received power documents for SiteId {siteData.SiteId} between {siteData.StartDateTime} and {siteData.EndDateTime}",
        new { context.InstanceId }
      );

      IEnumerable<Task> GetDailyDocuments()
      {
        var solarDays = GetSolarViewDays(siteData.SolarData);

        foreach (var solarDay in solarDays)
        {
          var powerDocument = new PowerDocument(siteData.SiteId, solarDay);

          Tracker.TrackInfo(
            $"Saving document {powerDocument.id} for SiteId {siteData.SiteId}, {powerDocument.Date.GetSolarDateString()}",
            new {context.InstanceId}
          );

          yield return powerDocuments.AddAsync(powerDocument);
        }
      }

      var tasks = GetDailyDocuments();

      await Task.WhenAll(tasks).ConfigureAwait(false);

      Tracker.TrackInfo($"All power documents for SiteId {siteData.SiteId} have been persisted");
    }

    private static IEnumerable<SolarViewDay> GetSolarViewDays(SolarData solarData)
    {
      // flattened list of data points - so we can group into days and meter types
      var meterPoints =
        from meter in solarData.PowerDetails.Meters
        let meterType = meter.Type.As<MeterType>()
        from value in meter.Values
        let timestamp = value.Date.ParseSolarDateTime()
        let watts = value.Value
        select new
        {
          timestamp.Date,
          MeterType = meterType,
          Timestamp = timestamp,
          Watts = watts
        };

      return
        from dailyMeterPoints in meterPoints.GroupBy(item => item.Date)
        select new SolarViewDay
        {
          Date = dailyMeterPoints.Key,
          Meters =
            from dailyMeterPoint in dailyMeterPoints.GroupBy(item => item.MeterType)
            select new SolarViewMeter
            {
              MeterType = dailyMeterPoint.Key,
              Points = dailyMeterPoint
                .OrderBy(item => item.Timestamp)
                .Select(item =>
                  new SolarViewMeterPoint
                  {
                    Timestamp = item.Timestamp,
                    Watts = item.Watts
                  })
            }
        };
    }
  }
}