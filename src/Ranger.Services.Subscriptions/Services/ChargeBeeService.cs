using System;
using System.Threading.Tasks;
using ChargeBee.Models;
using ChargeBee.Models.Enums;
using Newtonsoft.Json;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    public static class ChargeBeeService
    {
        public static async Task<LimitFields> GetSubscriptLimitDetailsAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
            {
                throw new ArgumentException($"{nameof(planId)} was null or whitespace.");
            }

            var entityResult = await Plan.Retrieve(planId).RequestAsync();
            return entityResult.Plan.MetaData.ToObject<LimitFields>(
                new JsonSerializer
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                });
        }

        public static async Task<string> GetHostedPageUrl(string subscriptionId, string planId)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentException($"{nameof(subscriptionId)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(planId))
            {
                throw new ArgumentException($"{nameof(planId)} was null or whitespace.");
            }

            var entityResult = await HostedPage.CheckoutExisting()
                                .SubscriptionId(subscriptionId)
                                .SubscriptionPlanId(planId)
                                .RequestAsync();

            return entityResult?.HostedPage?.Url;
        }

        public static async Task<TenantSubscription> CreateNewTenantSubscription(string pgsqlDatabaseUsername, string organizationName, string email, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(pgsqlDatabaseUsername))
            {
                throw new ArgumentException($"{nameof(pgsqlDatabaseUsername)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new ArgumentException($"{nameof(organizationName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException($"{nameof(email)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException($"{nameof(firstName)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException($"{nameof(lastName)} was null or whitespace.");
            }

            var entityResult = await Subscription.Create()
                                .PlanId("sandbox")
                                .AutoCollection(AutoCollectionEnum.Off)
                                .CustomerCompany(organizationName)
                                .CustomerId(pgsqlDatabaseUsername)
                                .CustomerFirstName(firstName)
                                .CustomerLastName(lastName)
                                .CustomerEmail(email)
                                .RequestAsync();

            return new TenantSubscription
            {
                SubscriptionId = entityResult.Subscription.Id,
                PlanId = "sandbox",
                PgsqlDatabaseUsername = pgsqlDatabaseUsername,
                UtilizationDetails = new UtilizationDetails
                {
                    GeofenceCount = 0,
                    ProjectCount = 0,
                    IntegrationCount = 0,
                    AccountCount = 1
                }
            };
        }
    }
}