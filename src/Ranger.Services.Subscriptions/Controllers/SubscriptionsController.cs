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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get the tenant subscription");
                throw new ApiException("Failed to get the checkout hosted page url", StatusCodes.Status500InternalServerError);
            }

            RangerChargeBeeHostedPage result = null;
            var hostedPageUrl = await ChargeBeeService.GetHostedPageUrl(tenantSubscription.SubscriptionId, planId);
            if (hostedPageUrl is null)
            {
                logger.LogError($"Failed to get the checkout hosted page url from ChargeBee for subscription id '{tenantSubscription.SubscriptionId}'.");
                throw new ApiException("Failed to get the checkout hosted page url", StatusCodes.Status500InternalServerError);
            }
            result = new RangerChargeBeeHostedPage
            {
                Url = hostedPageUrl
            };
            return new ApiResponse("Successfully retrieved checkout hosted page url", result);
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
                logger.LogError(ex, $"Failed to retrieve subscription plan id for tenant id '{tenantId}'.");
                throw new ApiException("Failed to retrieve subscription plan id", StatusCodes.Status500InternalServerError);
            }
            return new ApiResponse("Successfully retrieved plan id", tenantSubscription.PlanId);
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
            LimitFields limit = null;
            LimitFields utilized = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId);
                limit = await ChargeBeeService.GetSubscriptLimitDetailsAsync(tenantSubscription.PlanId);
                utilized = new LimitFields
                {
                    Geofences = tenantSubscription.UtilizationDetails.GeofenceCount,
                    Integrations = tenantSubscription.UtilizationDetails.IntegrationCount,
                    Projects = tenantSubscription.UtilizationDetails.ProjectCount,
                    Accounts = tenantSubscription.UtilizationDetails.AccountCount
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve limit details from ChargeBee");
                throw new ApiException("Failed to retrieve limit details from ChargeBee", StatusCodes.Status500InternalServerError);
            }
            return new ApiResponse("Successfully retrieved subscription limit details", new SubscriptionLimitDetails { Limit = limit, Utilized = utilized });
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
                throw new ApiException(ex.Message, statusCode: StatusCodes.Status402PaymentRequired);
            }
            return new ApiResponse("Successfully incremented resource", newCount);
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
                throw new ApiException("All resources have been removed", statusCode: StatusCodes.Status304NotModified);
            }
            return new ApiResponse("Successfully decremented resource", newCount);
        }
    }
}