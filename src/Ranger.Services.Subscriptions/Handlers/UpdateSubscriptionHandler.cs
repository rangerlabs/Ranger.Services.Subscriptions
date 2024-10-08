using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Subscriptions.Data;
using Ranger.Services.Subscriptions.Messages.Events;
using StackExchange.Redis;

namespace Ranger.Services.Subscriptions.Handlers
{
    public class UpdateSubscriptionHandler : ICommandHandler<UpdateSubscription>
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsRepository repo;
        private readonly ILogger<UpdateSubscriptionHandler> logger;
        private readonly IDatabase _redisDb;

        public UpdateSubscriptionHandler(IBusPublisher busPublisher, SubscriptionsRepository repo, IConnectionMultiplexer connectionMultiplexer, ILogger<UpdateSubscriptionHandler> logger)
        {
            this.busPublisher = busPublisher;
            this.repo = repo;
            this.logger = logger;
            _redisDb = connectionMultiplexer.GetDatabase();
        }

        public async Task HandleAsync(UpdateSubscription message, ICorrelationContext context)
        {
            logger.LogInformation("Updating subscription for tenant with subscription Id {SubscriptionId}", message.SubscriptionId);

            var subscription = await repo.GetTenantSubscriptionBySubscriptionId(message.SubscriptionId);
            if (subscription is null)
            {
                throw new Exception("No tenant found for the provided tenant id");
            }

            if (subscription.PlanId == message.PlanId && subscription.Active == message.Active && subscription.ScheduledCancellationDate == message.ScheduledCancellationDate)
            {
                logger.LogDebug("The requested tenant subscription was equal to the existing tenant subscription and was likely a duplicate webhook event. Aborting update");
                return;
            }

            if (message.OccurredAt <= subscription.OccurredAt)
            {
                logger.LogDebug("The requested tenant subscription occurred before the existing tenant subscription. Aborting update");
                return;
            }

            var planLimits = await ChargeBeeService.GetSubscriptLimitDetailsAsync(message.PlanId);
            subscription.OccurredAt = message.OccurredAt;
            subscription.PlanId = message.PlanId;
            subscription.Active = message.Active;
            subscription.ScheduledCancellationDate = message.ScheduledCancellationDate;
            subscription.PlanLimits = planLimits;
            await repo.UpdateTenantSubscriptionByTenantId(subscription.TenantId, subscription);
            await _redisDb.KeyDeleteAsync(RedisKeys.SubscriptionEnabled(subscription.TenantId));
            logger.LogDebug("Removed subscription status from cache");
            busPublisher.Publish(new SubscriptionUpdated(subscription.TenantId), context);
        }
    }
}