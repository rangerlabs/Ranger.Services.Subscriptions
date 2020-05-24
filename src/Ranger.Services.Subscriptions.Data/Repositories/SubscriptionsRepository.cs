using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ranger.Common;

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

            subscription.OccurredAt = DateTime.UtcNow;
            context.TenantSubscriptions.Add(subscription);
            await context.SaveChangesAsync();

        }

        public async Task<int> UpdateTenantSubscriptionByTenantId(string tenantId, TenantSubscription tenantSubscription)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            context.Update(tenantSubscription);
            return await context.SaveChangesAsync();
        }

        public async Task<TenantSubscription> GetTenantSubscriptionByTenantId(string tenantId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            var result = await context.TenantSubscriptions
                .Where(_ => _.TenantId == tenantId)
                .Include(_ => _.PlanLimits)
                .SingleOrDefaultAsync();
            return result;
        }

        public async Task<TenantSubscription> GetTenantSubscriptionByCustomerId(string customerId)
        {
            if (String.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException($"{nameof(customerId)} was null or whitespace");
            }

            var result = await context.TenantSubscriptions
                .Where(_ => _.CustomerId == customerId)
                .Include(_ => _.PlanLimits)
                .SingleOrDefaultAsync();
            if (result is null)
            {
                throw new RangerException("No tenant found for the provided customer id");
            }
            return result;
        }

        public async Task<TenantSubscription> GetTenantSubscriptionBySubscriptionId(string subscriptionId)
        {
            if (String.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentException($"{nameof(subscriptionId)} was null or whitespace");
            }

            var result = await context.TenantSubscriptions
                .Where(_ => _.SubscriptionId == subscriptionId)
                .Include(_ => _.PlanLimits)
                .SingleOrDefaultAsync();
            if (result is null)
            {
                throw new RangerException("No tenant found for the provided subscription id");
            }
            return result;
        }

        public async Task<IEnumerable<TenantSubscription>> GetAllTenantSubscriptions()
        {
            return await context.TenantSubscriptions.ToListAsync();
        }
    }
}