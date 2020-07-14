using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SolarViewBlazor.Events
{
  public class EventAggregator : IEventAggregator
  {
    private readonly IDictionary<Type, IList<ISubscription>> _subscriptions = new Dictionary<Type, IList<ISubscription>>();
    private readonly IDictionary<Type, IList<IAsyncSubscription>> _asyncSubscriptions = new Dictionary<Type, IList<IAsyncSubscription>>();

    public Task PublishAsync<TMessage>(TMessage message)
    {
      var tasks = new List<Task>();

      if (_asyncSubscriptions.TryGetValue(typeof(TMessage), out var asyncSubscriptions))
      {
        foreach (var subscription in asyncSubscriptions)
        {
          var task = subscription.HandleAsync(message);
          tasks.Add(task);
        }
      }

      if (_subscriptions.TryGetValue(typeof(TMessage), out var subscriptions))
      {
        foreach (var subscription in subscriptions)
        {
          subscription.Handle(message);
        }
      }

      return tasks.Count == 0
        ? Task.CompletedTask
        : Task.WhenAll(tasks);
    }

    public void Subscribe<TMessage>(Action<TMessage> handler, bool weakSubscription = true)
    {
      var subscription = weakSubscription
        ? (ISubscription)new WeakSubscription(handler)
        : new Subscription(handler);

      Subscribe<TMessage>(subscription);
    }

    public void Subscribe<TMessage>(Func<TMessage, Task> handler, bool weakSubscription = true)
    {
      var subscription = weakSubscription
        ? (IAsyncSubscription)new AsyncWeakSubscription(handler)
        : new AsyncSubscription(handler);

      Subscribe<TMessage>(subscription);
    }

    public void Unsubscribe<TMessage>(Action<TMessage> handler)
    {
      if (_subscriptions.TryGetValue(typeof(TMessage), out var subscriptions))
      {
        foreach (var subscription in subscriptions)
        {
          var action = subscription.GetHandler<TMessage>();

          if (action == handler)
          {
            subscriptions.Remove(subscription);
            return;
          }
        }
      }
    }

    public void Unsubscribe<TMessage>(Func<TMessage, Task> handler)
    {
      if (_asyncSubscriptions.TryGetValue(typeof(TMessage), out var subscriptions))
      {
        foreach (var subscription in subscriptions)
        {
          var action = subscription.GetHandler<TMessage>();

          if (action == handler)
          {
            subscriptions.Remove(subscription);
            return;
          }
        }
      }
    }

    private void Subscribe<TMessage>(ISubscription subscription)
    {
      if (_subscriptions.TryGetValue(typeof(TMessage), out var subscriptions))
      {
        subscriptions.Add(subscription);
      }
      else
      {
        subscriptions = new List<ISubscription> { subscription };
        _subscriptions.Add(typeof(TMessage), subscriptions);
      }
    }

    private void Subscribe<TMessage>(IAsyncSubscription subscription)
    {
      if (_asyncSubscriptions.TryGetValue(typeof(TMessage), out var subscriptions))
      {
        subscriptions.Add(subscription);
      }
      else
      {
        subscriptions = new List<IAsyncSubscription> { subscription };
        _asyncSubscriptions.Add(typeof(TMessage), subscriptions);
      }
    }
  }
}