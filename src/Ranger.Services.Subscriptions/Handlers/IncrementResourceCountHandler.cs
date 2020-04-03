using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    public class IncrementResourceCountHandler : ICommandHandler<IncrementResourceCount>
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsRepository subscriptionsRepository;
        private readonly ILogger<IncrementResourceCountHandler> logger;
        private readonly ITenantsClient tenantsClient;

        public IncrementResourceCountHandler(IBusPublisher busPublisher, SubscriptionsRepository subscriptionsRepository, ITenantsClient tenantsClient, ILogger<IncrementResourceCountHandler> logger)
        {
            this.tenantsClient = tenantsClient;
            this.logger = logger;
            this.subscriptionsRepository = subscriptionsRepository;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(IncrementResourceCount message, ICorrelationContext context)
        {
            ContextTenant tenant = null;
            try
            {
                tenant = await this.tenantsClient.GetTenantAsync<ContextTenant>(message.Domain);
            }
            catch (HttpClientException ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    throw new Exception($"No tenant found for domain {message.Domain}.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An exception occurred retrieving the ContextTenant object. Cannot construct the tenant specific repository.");
                throw;
            }

            int newCount = 0;
            try
            {
                var tenantSubscription = await subscriptionsRepository.GetTenantSubscriptionByPgsqlDatabaseUsername(tenant.DatabaseUsername);
                LimitFields limitDetails = null;
                try
                {
                    limitDetails = await ChargeBeeService.GetSubscriptLimitDetailsAsync(tenantSubscription.PlanId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to retrieve subscription limit details from ChargeBee. Permitting possible excess resource creation.");
                }

                switch (message.Resource)
                {
                    case ResourceEnum.Project:
                        {
                            newCount = ++tenantSubscription.UtilizationDetails.ProjectCount;
                            if (newCount < 0)
                            {
                                throw new RangerException("The request would exceed the Project subscription limit.");
                            }
                            break;
                        }
                    case ResourceEnum.Integration:
                        {
                            newCount = ++tenantSubscription.UtilizationDetails.IntegrationCount;
                            if (newCount < 0)
                            {
                                throw new RangerException("The request would exceed the Integration subscription limit.");
                            }
                            break;
                        }
                    case ResourceEnum.Geofence:
                        {
                            newCount = ++tenantSubscription.UtilizationDetails.GeofenceCount;
                            if (newCount < 0)
                            {
                                throw new RangerException("The request would exceed the Geofence subscription limit.");
                            }
                            break;
                        }
                    case ResourceEnum.Account:
                        {
                            newCount = ++tenantSubscription.UtilizationDetails.AccountCount;
                            if (newCount < 0)
                            {
                                throw new RangerException("The request would exceed the Account subscription limit.");
                            }
                            break;
                        }
                    default:
                        throw new Exception("The resource was not valid.");
                }

                await subscriptionsRepository.UpdateTenantSubscriptionByPgsqlDatabaseUsername(tenant.DatabaseUsername, tenantSubscription);
            }
            catch (Exception ex)
            {
                throw new RangerException("Failed to increment the resource count.", ex);
            }
            busPublisher.Publish(new ResourceCountIncremented(message.Domain, message.Resource, newCount), context);
        }
    }
}