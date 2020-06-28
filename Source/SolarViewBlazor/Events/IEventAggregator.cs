using System;

namespace SolarViewBlazor.Events
{
  public interface IEventAggregator
  {
    void Publish<TMessage>(TMessage message);
    void Subscribe<TMessage>(Action<TMessage> handler, bool weakSubscription = true);
    void Unsubscribe<TMessage>(Action<TMessage> handler);
  }
}