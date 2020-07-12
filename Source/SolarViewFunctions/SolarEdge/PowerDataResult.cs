using AllOverIt.Helpers;
using SolarViewFunctions.SolarEdge.Dto.Response;
using System.Net;

namespace SolarViewFunctions.SolarEdge
{
  public class PowerDataResult
  {
    public HttpStatusCode StatusCode { get; }
    public PowerDataDto PowerData { get; }
    public bool IsError => StatusCode != HttpStatusCode.OK;

    public PowerDataResult(PowerDataDto powerData)
    {
      StatusCode = HttpStatusCode.OK;
      PowerData = powerData.WhenNotNull(nameof(powerData));
    }

    public static PowerDataResult Error(HttpStatusCode statusCode)
    {
      return new PowerDataResult(statusCode);
    }

    private PowerDataResult(HttpStatusCode statusCode)
    {
      StatusCode = statusCode;
    }
  }
}