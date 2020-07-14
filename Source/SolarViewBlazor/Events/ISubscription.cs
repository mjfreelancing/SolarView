using System;

namespace SolarViewBlazor.Events
{
  public interface ISubscription
  {
    Action<TMessage> GetHandler<TMessage>();
    void Handle<TMessage>(TMessage message);
  }
}