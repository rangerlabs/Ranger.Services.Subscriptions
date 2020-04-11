using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    [ApiController]
    [ApiVersion("1.0")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly TenantsHttpClient tenantsClient;
        private readonly SubscriptionsRepository subscriptionsRepo;
        private readonly ILogger<SubscriptionsController> logger;
        private readonly ChargeBeeOptions options;
        private readonly SubscriptionsService subscriptionsService;

        public SubscriptionsController(TenantsHttpClient tenantsClient, SubscriptionsRepository subscriptionsRepo, SubscriptionsService subscriptionsService, ILogger<SubscriptionsController> logger, ChargeBeeOptions options)
        {
            this.subscriptionsService = subscriptionsService;
            this.tenantsClient = tenantsClient;
            this.subscriptionsRepo = subscriptionsRepo;
            this.options = options;
            this.logger = logger;
        }

        ///<summary>
        /// Gets the URL for the tenant's checkout page based on their current subscription
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        ///<param name="planId">The tenant's current ChargeBee plan id</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/subscriptions/{tenantId}/{planId}/checkout-existing-hosted-page-url")]
        public async Task<ApiResponse> GetCheckoutExistingHostedPageUrl(string tenantId, string planId)
        {
            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId);
                RangerChargeBeeHostedPage result = null;
                try
                {
                    var hostedPageUrl = await ChargeBeeService.GetHostedPageUrl(tenantSubscription.SubscriptionId, planId);
                    if (hostedPageUrl is null)
                    {
                        throw new Exception("Hosted Page Url response was null.");
                    }
                    result = new RangerChargeBeeHostedPage
                    {
                        Url = hostedPageUrl
                    };
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to get the hosted page for Subscription Id '{tenantSubscription.SubscriptionId}'.");
                    throw new ApiException("Internal Server Error", StatusCodes.Status500InternalServerError);
                }
                return new ApiResponse(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve subscription id for tenant id '{tenantId}'");
                throw new ApiException("Internal Server Error", StatusCodes.Status500InternalServerError);
            }
        }

        ///<summary>
        /// Gets the ChargeBee plan id for the requested tenant
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/subscriptions/{tenantId}/plan-id")]
        public async Task<ApiResponse> GetSubscription(string tenantId)
        {
            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve SubscriptionId for TenantId '{tenantId}'.");
                throw new ApiException("Internal Server Error", StatusCodes.Status500InternalServerError);
            }
            return new ApiResponse("Success", tenantSubscription.PlanId);
        }

        ///<summary>
        /// Gets the requested tenant's subscription limits
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/subscriptions{tenantId}/limit-details")]
        public async Task<ApiResponse> GetLimitDetails(string tenantId)
        {
            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve SubscriptionId for TenantId '{tenantId}'.");
                throw new ApiException("Internal Server Error", StatusCodes.Status500InternalServerError);
            }

            LimitFields limit = null;
            LimitFields utilized = null;
            try
            {
                limit = await ChargeBeeService.GetSubscriptLimitDetailsAsync(tenantSubscription.PlanId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve limit details from ChargeBee.");
                throw new ApiException("Internal Server Error", StatusCodes.Status500InternalServerError);
            }
            try
            {
                var _ = await subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId);
                utilized = new LimitFields
                {
                    Geofences = _.UtilizationDetails.GeofenceCount,
                    Integrations = _.UtilizationDetails.IntegrationCount,
                    Projects = _.UtilizationDetails.ProjectCount,
                    Accounts = _.UtilizationDetails.AccountCount
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve utilized limit details.");
                throw new ApiException("Internal Server Error", StatusCodes.Status500InternalServerError);
            }
            return new ApiResponse("Success", new SubscriptionLimitDetails { Limit = limit, Utilized = utilized });
        }

        ///<summary>
        /// Increments the requested tenant's specified resource
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        ///<param name="resourceModel">Represents the resource to increment</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
        [HttpPut("/subscriptions/{tenantId}/resources/increment")]
        public async Task<ApiResponse> IncrementSubscription(string tenantId, ResourceModel resourceModel)
        {
            int newCount = 0;
            try
            {
                newCount = await subscriptionsService.IncrementResource(tenantId, resourceModel.Resource);
            }
            catch (RangerException ex)
            {
                return new ApiResponse(ex.Message, statusCode: StatusCodes.Status402PaymentRequired);
            }
            return new ApiResponse("Success", newCount);
        }

        ///<summary>
        /// Decrements the requested tenant's specified resource
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        ///<param name="resourceModel">Represents the resource to increment</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [HttpPut("/subscriptions/{tenantId}/resources/decrement")]
        public async Task<ApiResponse> DecrementSubscription(string tenantId, ResourceModel resourceModel)
        {
            int newCount = 0;
            try
            {
                newCount = await subscriptionsService.DecrementResource(tenantId, resourceModel.Resource);
            }
            catch (RangerException)
            {
                return new ApiResponse("All resources have been removed", 0, StatusCodes.Status304NotModified);
            }
            return new ApiResponse("Success", newCount);
        }
    }
}