using AllOverIt.Helpers;
using SolarViewFunctions.SolarEdge.Dto.Response;
using System.Net;

namespace SolarViewFunctions.SolarEdge
{
  public class SolarDataResponse
  {
    public HttpStatusCode StatusCode { get; }
    public SolarData SolarData { get; }
    public bool IsError => StatusCode != HttpStatusCode.OK;

    public SolarDataResponse(SolarData solarData)
    {
      StatusCode = HttpStatusCode.OK;
      SolarData = solarData.WhenNotNull(nameof(solarData));
    }

    public static SolarDataResponse Error(HttpStatusCode statusCode)
    {
      return new SolarDataResponse(statusCode);
    }

    private SolarDataResponse(HttpStatusCode statusCode)
    {
      StatusCode = statusCode;
    }
  }
}