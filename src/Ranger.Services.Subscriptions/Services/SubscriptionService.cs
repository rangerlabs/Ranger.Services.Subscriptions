using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions {
    public class SubscriptionsService {
        private readonly SubscriptionsRepository subscriptionsRepository;
        private readonly ILogger<SubscriptionsRepository> logger;

        public SubscriptionsService (SubscriptionsRepository subscriptionsRepository, ILogger<SubscriptionsRepository> logger) {
            this.logger = logger;
            this.subscriptionsRepository = subscriptionsRepository;
        }

        public async Task<int> IncrementResource (string tenantId, ResourceEnum resource) {
            int newCount = 0;
            var tenantSubscription = await subscriptionsRepository.GetTenantSubscriptionByTenantId (tenantId);
            LimitFields limitDetails = null;
            try {
                limitDetails = await ChargeBeeService.GetSubscriptLimitDetailsAsync (tenantSubscription.PlanId);
            } catch (Exception ex) {
                logger.LogError (ex, "Failed to retrieve subscription limit details from ChargeBee. Permitting possible excess resource creation");
            }

            switch (resource) {
                case ResourceEnum.Project:
                    {
                        newCount = ++tenantSubscription.UtilizationDetails.ProjectCount;
                        if (newCount > limitDetails.Projects) {
                            throw new RangerException ("Project subscription limit met");
                        }
                        break;
                    }
                case ResourceEnum.Integration:
                    {
                        newCount = ++tenantSubscription.UtilizationDetails.IntegrationCount;
                        if (newCount > limitDetails.Integrations) {
                            throw new RangerException ("Integration subscription limit met");
                        }
                        break;
                    }
                case ResourceEnum.Geofence:
                    {
                        newCount = ++tenantSubscription.UtilizationDetails.GeofenceCount;
                        if (newCount > limitDetails.Geofences) {
                            throw new RangerException ("Geofence subscription limit met");
                        }
                        break;
                    }
                case ResourceEnum.Account:
                    {
                        newCount = ++tenantSubscription.UtilizationDetails.AccountCount;
                        if (newCount > limitDetails.Accounts) {
                            throw new RangerException ("Account subscription limit met");
                        }
                        break;
                    }
                default:
                    throw new ArgumentException ("The resource was not valid");
            }
            try {
                await subscriptionsRepository.UpdateTenantSubscriptionByTenantId (tenantId, tenantSubscription);
            } catch (Exception ex) {
                throw new RangerException ("Failed to increment the resource count", ex);
            }
            return newCount;
        }

        public async Task<int> DecrementResource (string tenantId, ResourceEnum resource) {
            var tenantSubscription = await subscriptionsRepository.GetTenantSubscriptionByTenantId (tenantId);

            int newCount = 0;
            switch (resource) {
                case ResourceEnum.Project:
                    {
                        newCount = --tenantSubscription.UtilizationDetails.ProjectCount;
                        if (newCount < 0) {
                            throw new RangerException ("There is no utilization to decrement");
                        }
                        break;
                    }
                case ResourceEnum.Integration:
                    {
                        newCount = --tenantSubscription.UtilizationDetails.IntegrationCount;
                        if (newCount < 0) {
                            throw new RangerException ("There is no utilization to decrement");
                        }
                        break;
                    }
                case ResourceEnum.Geofence:
                    {
                        newCount = --tenantSubscription.UtilizationDetails.GeofenceCount;
                        if (newCount < 0) {
                            throw new RangerException ("There is no utilization to decrement");
                        }
                        break;
                    }
                case ResourceEnum.Account:
                    {
                        newCount = --tenantSubscription.UtilizationDetails.AccountCount;
                        if (newCount < 0) {
                            throw new RangerException ("There is no utilization to decrement");
                        }
                        break;
                    }
                default:
                    throw new Exception ("The resource was not valid");
            }
            try {
                await subscriptionsRepository.UpdateTenantSubscriptionByTenantId (tenantId, tenantSubscription);
            } catch (Exception ex) {
                throw new RangerException ("Failed to increment the resource count", ex);
            }

            await subscriptionsRepository.UpdateTenantSubscriptionByTenantId (tenantId, tenantSubscription);
            return newCount;
        }
    }
}