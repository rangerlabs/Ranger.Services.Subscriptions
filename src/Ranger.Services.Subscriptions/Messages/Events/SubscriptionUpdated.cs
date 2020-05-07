using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions.Messages.Events
{
    [MessageNamespace("subscriptions")]
    public class SubscriptionUpdated : IEvent
    {
        public SubscriptionUpdated()
        { }
    }
}