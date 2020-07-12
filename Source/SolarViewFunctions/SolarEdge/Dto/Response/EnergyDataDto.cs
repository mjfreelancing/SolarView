namespace SolarViewFunctions.SolarEdge.Dto.Response
{
  public class EnergyDataDto
  {
    // This must be called EnergyDetails as it is the response from SolarEdge.
    // This is internally mapped to MeterValues on the SolarData model.
    public EnergyDetailsDto EnergyDetails { get; set; }
  }
}