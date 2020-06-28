using System;
using System.Collections.Generic;

namespace SolarViewBlazor.Events
{
  public class EventAggregator : IEventAggregator
  {
    private readonly IDictionary<Type, IList<ISubscription>> _subscriptions = new Dictionary<Type, IList<ISubscription>>();

    public void Publish<TMessage>(TMessage message)
    {
      if (_subscriptions.TryGetValue(typeof(TMessage), out var subscriptions))
      {
        foreach (var subscription in subscriptions)
        {
          subscription.Handle(message);
        }
      }
    }

    public void Subscribe<TMessage>(Action<TMessage> handler, bool weakSubscription = true)
    {
      var subscription = weakSubscription
        ? (ISubscription)new WeakSubscription(handler)
        : new Subscription(handler);

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
  }
}