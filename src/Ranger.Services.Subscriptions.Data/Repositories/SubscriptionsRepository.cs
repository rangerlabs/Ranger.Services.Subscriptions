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

        public async Task UpdateTenantSubscriptionByTenantId(string tenantId, TenantSubscription tenantSubscription)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }

            var existing = await context.TenantSubscriptions.Where(_ => _.TenantId == tenantId).SingleAsync();
            existing.UtilizationDetails = tenantSubscription.UtilizationDetails;
            existing.PlanId = tenantSubscription.PlanId;
            context.Update(existing);
            await context.SaveChangesAsync();
        }

        public async Task<TenantSubscription> GetTenantSubscriptionByTenantId(string tenantId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace.");
            }

            return await context.TenantSubscriptions
                            .Where(_ => _.TenantId == tenantId)
                            .Include(_ => _.UtilizationDetails)
                            .SingleAsync();
        }
    }
}