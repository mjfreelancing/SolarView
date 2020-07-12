namespace SolarViewFunctions.SolarEdge.Dto.Response
{
  public class PowerDataDto
  {
    // This must be called PowerDetails as it is the response from SolarEdge.
    // This is internally mapped to MeterValues on the SolarData model.
    public PowerDetailsDto PowerDetails { get; set; }
  }
}