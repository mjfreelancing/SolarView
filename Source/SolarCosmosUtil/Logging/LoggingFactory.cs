using AllOverIt.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace SolarCosmosUtil.Logging
{
  public class LoggingFactory : ILoggingFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public LoggingFactory(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider.WhenNotNull(nameof(serviceProvider));
    }

    public ILogger CreateLogger(object instance)
    {
      return CreateLogger(instance.GetType());
    }

    public ILogger CreateLogger(Type type)
    {
      return _serviceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(type)) as ILogger;
    }
  }
}