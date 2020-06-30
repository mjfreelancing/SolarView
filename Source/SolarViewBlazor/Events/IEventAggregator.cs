using System;
using System.Threading.Tasks;

namespace SolarViewBlazor.Events
{
  public interface IEventAggregator
  {
    Task PublishAsync<TMessage>(TMessage message);
    void Subscribe<TMessage>(Action<TMessage> handler, bool weakSubscription = true);
    void Subscribe<TMessage>(Func<TMessage, Task> handler, bool weakSubscription = true);
    void Unsubscribe<TMessage>(Action<TMessage> handler);
    void Unsubscribe<TMessage>(Func<TMessage, Task> handler);
  }
}