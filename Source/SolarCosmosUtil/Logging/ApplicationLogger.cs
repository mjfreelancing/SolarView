using AllOverIt.Helpers;
using Microsoft.Extensions.Logging;
using SolarCosmosUtil.Telemetry;

namespace SolarCosmosUtil.Logging
{


  public class ApplicationLoggerWithTelemetry : IApplicationLogger
  {
    private readonly IApplicationLogger _logger;
    private readonly ITelemetryTracker _tracker;

    public ApplicationLoggerWithTelemetry(IApplicationLogger logger, ITelemetryTracker tracker)
    {
      _logger = logger.WhenNotNull(nameof(logger));
      _tracker = tracker.WhenNotNull(nameof(tracker));
    }

    public void Trace<TType>(string message)
    {
      _logger.Trace<TType>(message);
      _tracker.Client.TrackTrace(message);
    }

    public void Debug<TType>(string message)
    {
      _logger.Debug<TType>(message);
    }

    public void Info<TType>(string message)
    {
      _logger.Info<TType>(message);
    }

    public void Warn<TType>(string message)
    {
      _logger.Warn<TType>(message);
    }

    public void Error<TType>(string message)
    {
      _logger.Error<TType>(message);
    }

    public void Critical<TType>(string message)
    {
      _logger.Critical<TType>(message);
    }
  }




  public class ApplicationLogger : IApplicationLogger
  {
    private readonly ILoggingFactory _loggingFactory;

    public ApplicationLogger(ILoggingFactory loggingFactory)
    {
      _loggingFactory = loggingFactory.WhenNotNull(nameof(loggingFactory));
    }

    public void Trace<TType>(string message)
    {
      var logger = _loggingFactory.CreateLogger(typeof(TType));
      logger.LogTrace(message);
    }

    public void Debug<TType>(string message)
    {
      var logger = _loggingFactory.CreateLogger(typeof(TType));
      logger.LogDebug(message);
    }

    public void Info<TType>(string message)
    {
      var logger = _loggingFactory.CreateLogger(typeof(TType));
      logger.LogInformation(message);
    }

    public void Warn<TType>(string message)
    {
      var logger = _loggingFactory.CreateLogger(typeof(TType));
      logger.LogWarning(message);
    }

    public void Error<TType>(string message)
    {
      var logger = _loggingFactory.CreateLogger(typeof(TType));
      logger.LogError(message);
    }

    public void Critical<TType>(string message)
    {
      var logger = _loggingFactory.CreateLogger(typeof(TType));
      logger.LogCritical(message);
    }
  }
}