using System;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    public class SubscriptionLimitDetails
    {
        public string PlanId { get; set; }
        public PlanLimits Utilized { get; set; }
        public PlanLimits Limit { get; set; }
        public bool Active { get; set; }
        public DateTime? ScheduledCancellationDate { get; set; }
    }
}