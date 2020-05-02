using System.Collections.Generic;
using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class TenantLimitDetailsComputed : IEvent
    {
        public IEnumerable<(string tenantId, LimitFields limits)> TenantLimitDetails { get; set; }

        public TenantLimitDetailsComputed(IEnumerable<(string tenantId, LimitFields limits)> tenantLimitDetails)
        {
            TenantLimitDetails = tenantLimitDetails;
        }
    }
}