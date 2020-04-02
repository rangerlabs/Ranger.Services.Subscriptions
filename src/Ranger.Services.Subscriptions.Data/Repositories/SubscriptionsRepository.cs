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

            context.TenantSubscriptions.Add(subscription);
            await context.SaveChangesAsync();

        }

        public async Task UpdateTenantSubscriptionByPgsqlDatabaseUsername(string pgsqlDatabaseUsername, TenantSubscription tenantSubscription)
        {
            if (string.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }

            var existing = await context.TenantSubscriptions.Where(_ => _.PgsqlDatabaseUsername == pgsqlDatabaseUsername).SingleAsync();
            existing.UtilizationDetails = tenantSubscription.UtilizationDetails;
            existing.PlanId = tenantSubscription.PlanId;
            context.Update(existing);
            await context.SaveChangesAsync();
        }

        public async Task<TenantSubscription> GetTenantSubscriptionByPgsqlDatabaseUsername(string pgsqlDatabaseUsername)
        {
            if (String.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }

            return await context.TenantSubscriptions
                            .Where(_ => _.PgsqlDatabaseUsername == pgsqlDatabaseUsername)
                            .Include(_ => _.UtilizationDetails)
                            .SingleAsync();
        }
    }
}