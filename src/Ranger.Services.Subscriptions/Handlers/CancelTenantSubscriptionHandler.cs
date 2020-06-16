using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Subscriptions.Commands;
using Ranger.Services.Operations.Messages.Subscriptions.Events;
using Ranger.Services.Subscriptions.Data;
using Ranger.Services.Subscriptions.Messages.Events;

namespace Ranger.Services.Subscriptions.Handlers
{
    public class CancelTenantSubscriptionHandler : ICommandHandler<CancelTenantSubscription>
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsRepository subscriptionsRepository;
        private readonly ILogger<UpdateTenantSubscriptionOrganizationHandler> logger;

        public CancelTenantSubscriptionHandler(IBusPublisher busPublisher, SubscriptionsRepository subscriptionsRepository, ILogger<UpdateTenantSubscriptionOrganizationHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.subscriptionsRepository = subscriptionsRepository;
            this.logger = logger;
        }

        public async Task HandleAsync(CancelTenantSubscription message, ICorrelationContext context)
        {
            try
            {
                var subscription = await subscriptionsRepository.GetTenantSubscriptionByTenantId(message.TenantId);
                await ChargeBeeService.CancelChargeBeeSubscription(subscription.SubscriptionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred cancelling the Chargebee subscription for TenantId {TenantId}", message.TenantId, message.OrganizationName);
                throw;
            }
            busPublisher.Publish(new TenantSubscriptionCancelled(message.TenantId), context);
        }
    }
}