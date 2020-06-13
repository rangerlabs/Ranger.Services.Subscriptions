using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.Services.Operations.Messages.Subscriptions.Commands;
using Ranger.Services.Operations.Messages.Subscriptions.Events;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions.Handlers
{
    public class UpdateTenantSubscriptionOrganizationHandler : ICommandHandler<UpdateTenantSubscriptionOrganization>
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsRepository subscriptionsRepository;
        private readonly ILogger<UpdateTenantSubscriptionOrganizationHandler> logger;

        public UpdateTenantSubscriptionOrganizationHandler(IBusPublisher busPublisher, SubscriptionsRepository subscriptionsRepository, ILogger<UpdateTenantSubscriptionOrganizationHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.subscriptionsRepository = subscriptionsRepository;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateTenantSubscriptionOrganization message, ICorrelationContext context)
        {
            try
            {
                var subscription = await subscriptionsRepository.GetTenantSubscriptionByTenantId(message.TenantId);
                await ChargeBeeService.UpdateChargebeeCustomerOrganization(subscription.CustomerId, message.OrganizationName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred updating the Chargebee organization details for TenantId {TenantId}. Expected to set Organization Name to {OrganizationName}", message.TenantId, message.OrganizationName);
                throw;
            }
            busPublisher.Publish(new TenantSubscriptionOrganizationUpdated(), context);
        }
    }
}