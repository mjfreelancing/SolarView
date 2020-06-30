using System;
using System.Threading.Tasks;
using AllOverIt.Helpers;

namespace SolarViewBlazor.Events
{
  public class AsyncSubscription : IAsyncSubscription
  {
    private readonly Delegate _handler;

    public AsyncSubscription(Delegate handler)
    {
      _handler = handler.WhenNotNull(nameof(handler));
    }

    public Func<TMessage, Task> GetHandler<TMessage>()
    {
      return (Func<TMessage, Task>)_handler;
    }

    public Task HandleAsync<TMessage>(TMessage message)
    {
      return GetHandler<TMessage>().Invoke(message);
    }
  }
}