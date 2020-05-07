using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;
using Ranger.Services.Subscriptions.Data;
using Ranger.Services.Subscriptions.Messages.Events;

namespace Ranger.Services.Subscriptions.Handlers
{
    public class UpdateSubscriptionHandler : ICommandHandler<UpdateSubscription>
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsRepository repo;
        private readonly ILogger<UpdateSubscriptionHandler> logger;

        public UpdateSubscriptionHandler(IBusPublisher busPublisher, SubscriptionsRepository repo, ILogger<UpdateSubscriptionHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.repo = repo;
            this.logger = logger;
        }

        public async Task HandleAsync(UpdateSubscription message, ICorrelationContext context)
        {
            logger.LogInformation($"Updating subscription for tenant {message.TenantId}");
            await repo.UpdateTenantSubscriptionByTenantId(message.TenantId, new TenantSubscription { PlanId = message.PlanId, Active = message.Active });
            busPublisher.Publish(new SubscriptionUpdated(), context);
        }
    }
}