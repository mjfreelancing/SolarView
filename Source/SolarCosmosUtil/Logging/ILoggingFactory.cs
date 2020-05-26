using Microsoft.Extensions.Logging;
using System;

namespace SolarCosmosUtil.Logging
{
  public interface ILoggingFactory
  {
    ILogger CreateLogger(object instance);
    ILogger CreateLogger(Type type);
  }
}