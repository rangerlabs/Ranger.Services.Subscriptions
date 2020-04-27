namespace Ranger.Services.Subscriptions
{
    public class SubscriptionLimitDetails
    {
        public string PlanId { get; set; }
        public LimitFields Utilized { get; set; }
        public LimitFields Limit { get; set; }
    }
}