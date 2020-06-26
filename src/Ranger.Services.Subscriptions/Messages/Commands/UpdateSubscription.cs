using System;
using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("subscriptions")]
    public class UpdateSubscription : ICommand
    {
        public UpdateSubscription(string subscriptionId, string planId, DateTime occurredAt, bool active = true, DateTime? scheduledCancellationDate = null)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new System.ArgumentException($"{nameof(subscriptionId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(planId))
            {
                throw new System.ArgumentException($"{nameof(planId)} was null or whitespace");
            }

            this.SubscriptionId = subscriptionId;
            this.PlanId = planId;
            this.OccurredAt = occurredAt;
            this.Active = active;
            this.ScheduledCancellationDate = scheduledCancellationDate;
        }
        public string SubscriptionId { get; }
        public string PlanId { get; }
        public DateTime OccurredAt { get; }
        public bool Active { get; }
        public DateTime? ScheduledCancellationDate { get; }
    }
}