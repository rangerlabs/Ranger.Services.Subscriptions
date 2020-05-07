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

            context.TenantSubscriptions.Add(subscription);
            await context.SaveChangesAsync();

        }

        public async Task UpdateTenantSubscriptionByTenantId(string tenantId, TenantSubscription tenantSubscription)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            var existing = await context.TenantSubscriptions.Where(_ => _.TenantId == tenantId).SingleOrDefaultAsync();
            if (existing is null)
            {
                throw new RangerException("No tenant found for the provided tenant id");
            }
            existing.PlanId = tenantSubscription.PlanId;
            existing.Active = tenantSubscription.Active;
            existing.ScheduledCancellationDate = tenantSubscription.ScheduledCancellationDate;
            context.Update(existing);
            await context.SaveChangesAsync();
        }

        public async Task<TenantSubscription> GetTenantSubscriptionByTenantId(string tenantId)
        {
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }

            var result = await context.TenantSubscriptions
                .Where(_ => _.TenantId == tenantId)
                .SingleOrDefaultAsync();
            if (result is null)
            {
                throw new RangerException("No tenant found for the provided tenant id");
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