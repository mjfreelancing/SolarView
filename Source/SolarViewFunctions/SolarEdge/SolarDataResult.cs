using AllOverIt.Helpers;
using SolarViewFunctions.SolarEdge.Dto.Response;
using System.Net;

namespace SolarViewFunctions.SolarEdge
{
  public class SolarDataResult
  {
    public HttpStatusCode StatusCode { get; }
    public SolarDataDto SolarData { get; }
    public bool IsError => StatusCode != HttpStatusCode.OK;

    public SolarDataResult(SolarDataDto solarData)
    {
      StatusCode = HttpStatusCode.OK;
      SolarData = solarData.WhenNotNull(nameof(solarData));
    }

    public static SolarDataResult Error(HttpStatusCode statusCode)
    {
      return new SolarDataResult(statusCode);
    }

    private SolarDataResult(HttpStatusCode statusCode)
    {
      StatusCode = statusCode;
    }
  }
}