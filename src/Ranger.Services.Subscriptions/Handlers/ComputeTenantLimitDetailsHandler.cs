using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions.Handlers
{
    public class ComputeTenantLimitDetailsHandler : ICommandHandler<ComputeTenantLimitDetails>
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsRepository subscriptionsRepository;
        private readonly ILogger<ComputeTenantLimitDetailsHandler> logger;

        public ComputeTenantLimitDetailsHandler(IBusPublisher busPublisher, SubscriptionsRepository subscriptionsRepository, ILogger<ComputeTenantLimitDetailsHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.subscriptionsRepository = subscriptionsRepository;
            this.logger = logger;
        }

        public async Task HandleAsync(ComputeTenantLimitDetails message, ICorrelationContext context)
        {
            var allSubscriptionLimitDetails = await ChargeBeeService.GetAllSubscriptionLimitDetailsAsync();
            var allTenantSubscriptions = await subscriptionsRepository.GetAllTenantSubscriptions();

            var tenantLimitTuple = new List<(string, PlanLimits)>();
            foreach (var tenantId in message.TenantIds)
            {
                var subscription = allTenantSubscriptions.Where(t => t.TenantId == tenantId).Single();
                tenantLimitTuple.Add((tenantId, allSubscriptionLimitDetails.Where(r => r.Id == subscription.PlanId).Select(r => r.PlanLimits).Single()));
            }
            busPublisher.Publish(new TenantLimitDetailsComputed(tenantLimitTuple), context);
        }
    }
}