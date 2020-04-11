using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{

    public class CreateNewTenantSubscriptionHandler : ICommandHandler<CreateNewTenantSubscription>
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsRepository subscriptionsRepository;
        private readonly ILogger<CreateNewTenantSubscriptionHandler> logger;

        public CreateNewTenantSubscriptionHandler(IBusPublisher busPublisher, SubscriptionsRepository subscriptionsRepository, ILogger<CreateNewTenantSubscriptionHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.subscriptionsRepository = subscriptionsRepository;
            this.logger = logger;
        }

        // Don't throw a RejectedEvent here. This should be the last action in the TenantOnboardingSaga and we don't want
        // the tenant to be lost. Log the exceptions as Critical and manually requeue as soon as possible.
        public async Task HandleAsync(CreateNewTenantSubscription message, ICorrelationContext context)
        {
            TenantSubscription subscription = null;
            try
            {
                subscription = await ChargeBeeService.CreateNewTenantSubscription(
                        message.TenantId,
                        message.OrganizationName,
                        message.CommandingUserEmail,
                        message.FirstName,
                        message.LastName
                    );
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Failed to create the new sandbox subscription for tenant with domain {message.TenantId}.");
                throw;
            }

            try
            {
                await subscriptionsRepository.AddTenantSubscription(subscription);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Failed to persist the newly created subscription for tenant with domain {message.TenantId}.");
                throw;
            }
            busPublisher.Publish(new NewTenantSubscriptionCreated(), context);
        }
    }
}