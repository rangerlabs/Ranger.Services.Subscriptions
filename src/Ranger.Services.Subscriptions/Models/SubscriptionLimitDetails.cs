using System;

namespace Ranger.Services.Subscriptions
{
    public class SubscriptionLimitDetails
    {
        public string PlanId { get; set; }
        public LimitFields Utilized { get; set; }
        public LimitFields Limit { get; set; }
        public bool Active { get; set; }
        public DateTime? ScheduledCancellationDate { get; set; }
    }
}