using System.Collections.Generic;
using Ranger.RabbitMQ;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class TenantLimitDetailsComputed : IEvent
    {
        public IEnumerable<(string tenantId, PlanLimits limits)> TenantLimitDetails { get; set; }

        public TenantLimitDetailsComputed(IEnumerable<(string tenantId, PlanLimits limits)> tenantLimitDetails)
        {
            TenantLimitDetails = tenantLimitDetails;
        }
    }
}