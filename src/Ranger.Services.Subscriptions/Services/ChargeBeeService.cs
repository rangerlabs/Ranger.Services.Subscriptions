using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChargeBee.Models;
using ChargeBee.Models.Enums;
using Newtonsoft.Json;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    public static class ChargeBeeService
    {
        public static async Task<PlanLimits> GetSubscriptLimitDetailsAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
            {
                throw new ArgumentException($"{nameof(planId)} was null or whitespace");
            }
            var entityResult = await Plan.Retrieve(planId).RequestAsync();
            return entityResult.Plan.MetaData.ToObject<PlanLimits>(
                new JsonSerializer
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                });
        }

        public static async Task<Subscription> GetSubscriptionAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            var entityResult = await Customer.Retrieve(tenantId).RequestAsync();
            var subscription = await Subscription.Retrieve(entityResult.Subscription.Id).RequestAsync();
            return entityResult.Subscription;
        }

        public static async Task<IEnumerable<RangerPlan>> GetAllSubscriptionLimitDetailsAsync()
        {
            var listResult = await Plan.List().Status().Is(Plan.StatusEnum.Active).RequestAsync();
            var plans = new List<RangerPlan>();
            foreach (var item in listResult.List)
            {
                plans.Add(new RangerPlan(item.Plan.Id, item.Plan.MetaData.ToObject<PlanLimits>(new JsonSerializer { MissingMemberHandling = MissingMemberHandling.Error })));
            }
            return plans;
        }

        public static async Task<HostedPage> GetHostedPageUrl(string subscriptionId, string planId)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentException($"{nameof(subscriptionId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(planId))
            {
                throw new ArgumentException($"{nameof(planId)} was null or whitespace");
            }

            var entityResult = await HostedPage.CheckoutExisting()
                                .SubscriptionId(subscriptionId)
                                .SubscriptionPlanId(planId)
                                .RequestAsync();

            return entityResult?.HostedPage;
        }

        public static async Task<PortalSession> GetPortalSessionAsync(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException($"{nameof(customerId)} was null or whitespace");
            }
            var entityResult = await PortalSession.Create()
                                .CustomerId(customerId).RequestAsync();
            return entityResult?.PortalSession;
        }

        public static async Task<TenantSubscription> CreateNewTenantSubscription(string tenantId, string organizationName, string email, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new ArgumentException($"{nameof(organizationName)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException($"{nameof(firstName)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException($"{nameof(lastName)} was null or whitespace");
            }

            var entityResult = await Subscription.Create()
                                .PlanId("sandbox")
                                .AutoCollection(AutoCollectionEnum.Off)
                                .CustomerCompany(organizationName)
                                .CustomerId(tenantId)
                                .CustomerFirstName(firstName)
                                .CustomerLastName(lastName)
                                .CustomerEmail(email)
                                .RequestAsync();

            return new TenantSubscription
            {
                SubscriptionId = entityResult.Subscription.Id,
                CustomerId = entityResult.Customer.Id,
                PlanId = "sandbox",
                TenantId = tenantId,
                Active = true
            };
        }

        public static async Task UpdateChargebeeCustomerOrganization(string customerId, string organizationName)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException($"'{nameof(customerId)}' cannot be null or whitespace", nameof(customerId));
            }

            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new ArgumentException($"'{nameof(organizationName)}' cannot be null or whitespace", nameof(organizationName));
            }

            await Customer.Update(customerId).Company(organizationName).RequestAsync();
        }
    }
}