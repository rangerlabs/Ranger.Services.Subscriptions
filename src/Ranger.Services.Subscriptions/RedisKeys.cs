namespace Ranger.Services.Subscriptions
{
    public static class RedisKeys
    {
        public static string SubscriptionEnabled(string tenantId) => $"SUBSCRIPTION_ENABLED_${tenantId}";
    }
}