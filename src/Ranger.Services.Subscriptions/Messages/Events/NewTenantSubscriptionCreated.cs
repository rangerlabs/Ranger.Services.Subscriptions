
using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class NewTenantSubscriptionCreated : IEvent
    { }
}