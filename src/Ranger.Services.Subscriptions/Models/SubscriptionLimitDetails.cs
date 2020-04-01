namespace Ranger.Services.Subscriptions
{
    public class SubscriptionLimitDetails
    {
        public int GeofenceLimit { get; set; }
        public int IntegrationLimit { get; set; }
        public int ProjectLimit { get; set; }
        public int AccountLimit { get; set; }
        public int ActiveUserLimit { get; set; }
    }
}