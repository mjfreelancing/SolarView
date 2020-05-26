namespace SolarCosmosUtil.Logging
{
  public interface IApplicationLogger
  {
    void Trace<TType>(string message);
    void Debug<TType>(string message);
    void Info<TType>(string message);
    void Warn<TType>(string message);
    void Error<TType>(string message);
    void Critical<TType>(string message);
  }
}