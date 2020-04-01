using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ranger.Services.Subscriptions.Data
{
    public class SubscriptionsRepository
    {
        private readonly SubscriptionsDbContext context;
        private readonly ILogger<SubscriptionsRepository> logger;

        public SubscriptionsRepository(SubscriptionsDbContext context, ILogger<SubscriptionsRepository> logger)
        {
            this.logger = logger;
            this.context = context;
        }

        public async Task AddTenantSubscription(TenantSubscription subscription)
        {
            if (subscription is null)
            {
                throw new ArgumentNullException(nameof(subscription));
            }

            try
            {
                context.TenantSubscriptions.Add(subscription);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to save the new tenant subscription for the domain with Database Username {subscription.PgsqlDatabaseUsername}.");
                throw;
            }
        }

        public async Task<TenantSubscription> GetTenantSubscriptionByPgsqlDatabaseUsername(string pgsqlDatabaseUsername)
        {
            if (String.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }

            return await context.TenantSubscriptions.Where(_ => _.PgsqlDatabaseUsername == pgsqlDatabaseUsername).SingleAsync();
        }
    }
}