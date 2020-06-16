using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class TenantSubscriptionCancelled : IEvent
    {
        public string TenantId { get; }

        public TenantSubscriptionCancelled(string tenantId)
        {
            this.TenantId = tenantId;
        }
    }
}