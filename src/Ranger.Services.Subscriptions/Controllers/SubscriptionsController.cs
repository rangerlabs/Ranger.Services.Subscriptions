using System;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Subscriptions.Data;
using StackExchange.Redis;

namespace Ranger.Services.Subscriptions
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ITenantsHttpClient tenantsClient;
        private readonly SubscriptionsRepository subscriptionsRepo;
        private readonly ILogger<SubscriptionsController> logger;
        private readonly ChargeBeeOptions options;
        private readonly SubscriptionsService subscriptionsService;
        private readonly IDatabase _redisDb;

        public SubscriptionsController(ITenantsHttpClient tenantsClient, SubscriptionsRepository subscriptionsRepo, SubscriptionsService subscriptionsService, IConnectionMultiplexer connectionMultiplexer, ILogger<SubscriptionsController> logger, ChargeBeeOptions options)
        {
            this.subscriptionsService = subscriptionsService;
            _redisDb = connectionMultiplexer.GetDatabase();
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
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/{tenantId}/{planId}/checkout-existing-hosted-page-url")]
        public async Task<ApiResponse> GetCheckoutExistingHostedPageUrl(string tenantId, string planId, CancellationToken cancellationToken = default(CancellationToken))
        {
            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId, cancellationToken);
                if (!tenantSubscription.Active)
                {
                    throw new ApiException("Cannot modify an inactive subscription", StatusCodes.Status400BadRequest);
                }
                if (tenantSubscription is null)
                {
                    throw new ApiException("No subscription was found for the provided tenant id", StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get the tenant subscription");
                throw new ApiException("Failed to get the checkout hosted page url", StatusCodes.Status500InternalServerError);
            }

            try
            {
                logger.LogInformation("Retrieving hosted page url for {SubscriptionId} and {PlanId}", tenantSubscription.SubscriptionId, planId);
                var hostedPageUrl = await ChargeBeeService.GetHostedPageUrl(tenantSubscription.SubscriptionId, planId, cancellationToken);
                if (hostedPageUrl is null)
                {
                    logger.LogError($"Failed to get the checkout hosted page url from ChargeBee for subscription id '{tenantSubscription.SubscriptionId}'");
                    throw new ApiException("Failed to get the checkout hosted page url", StatusCodes.Status500InternalServerError);
                }
                return new ApiResponse("Successfully retrieved checkout hosted page url", hostedPageUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve hosted page url for {SubscriptionId} and {PlanId}", tenantSubscription.SubscriptionId, planId);
                throw new ApiException("Failed to retrieve hosted page url from Chargebee", StatusCodes.Status500InternalServerError);
            }
        }

        ///<summary>
        /// Gets the Portal Session for the tenant's portal page based on their current subscription
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/{tenantId}/portal-session")]
        public async Task<ApiResponse> GetPortalSession(string tenantId, CancellationToken cancellationToken)
        {
            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId, cancellationToken);
                if (tenantSubscription is null)
                {
                    throw new ApiException("No subscription was found for the provided customer id", StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get the tenant subscription");
                throw new ApiException("Failed to get the portal session", StatusCodes.Status500InternalServerError);
            }

            try
            {
                logger.LogInformation("Retrieving portal session for {CustomerId}", tenantSubscription.CustomerId);
                var portalSession = await ChargeBeeService.GetPortalSessionAsync(tenantSubscription.CustomerId, cancellationToken);
                if (portalSession is null)
                {
                    logger.LogError($"Failed to get the portal session from ChargeBee for customer id '{tenantSubscription.CustomerId}'");
                    throw new ApiException("Failed to get the portal session", StatusCodes.Status500InternalServerError);
                }
                return new ApiResponse("Successfully retrieved portal session", portalSession);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve portal session for {CustomerId}", tenantSubscription.CustomerId);
                throw new ApiException("Failed to retrieve portal session from Chargebee", StatusCodes.Status500InternalServerError);
            }
        }

        ///<summary>
        /// Gets the ChargeBee plan id for the requested tenant
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/{tenantId}/plan-id")]
        public async Task<ApiResponse> GetPlanId(string tenantId, CancellationToken cancellationToken)
        {
            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId, cancellationToken);
                if (tenantSubscription is null)
                {
                    throw new ApiException("No subscription was found for the provided tenant id", StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve subscription plan id for tenant id '{tenantId}'");
                throw new ApiException("Failed to retrieve subscription plan id", StatusCodes.Status500InternalServerError);
            }
            return new ApiResponse("Successfully retrieved plan id", tenantSubscription.PlanId);
        }


        ///<summary>
        /// Determines whether a subscription is active
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/{tenantId}/active")]
        public async Task<ApiResponse> IsSubscriptionActive(string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                string redisResult = await _redisDb.StringGetAsync(RedisKeys.SubscriptionEnabled(tenantId));
                if (!String.IsNullOrWhiteSpace(redisResult))
                {
                    logger.LogDebug("Retrieved subscription status from cache");
                    return new ApiResponse("Successfully determined whether subscription is active", bool.Parse(redisResult));
                }
                var tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId, cancellationToken);
                if (tenantSubscription is null)
                {
                    throw new ApiException("No subscription was found for the provided tenant id", StatusCodes.Status404NotFound);
                }
                await _redisDb.StringSetAsync(RedisKeys.SubscriptionEnabled(tenantId), tenantSubscription.Active.ToString());
                logger.LogDebug("Added subscription status to cache");
                return new ApiResponse("Successfully determined whether subscription is active", tenantSubscription.Active);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve subscription for tenant id '{tenantId}'");
                throw new ApiException("Failed to determine whether subscription is active", StatusCodes.Status500InternalServerError);
            }
        }

        ///<summary>
        /// Gets the requested tenant's subscription limits
        ///</summary>
        ///<param name="tenantId">The tenant's unique identifier</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/{tenantId}")]
        public async Task<ApiResponse> GetSubscription(string tenantId, CancellationToken cancellationToken)
        {
            TenantSubscription tenantSubscription = null;
            PlanLimits limit = null;
            PlanLimits utilized = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByTenantId(tenantId, cancellationToken);
            }
            catch (RangerException ex)
            {
                throw new ApiException(ex.Message, StatusCodes.Status402PaymentRequired);
            }
            try
            {
                limit = tenantSubscription.PlanLimits;
                utilized = await subscriptionsService.GetUtilizedLimitFields(tenantId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to determine limit details ");
                throw new ApiException("Failed to determine limit details ", StatusCodes.Status500InternalServerError);
            }
            return new ApiResponse("Successfully retrieved subscription", new SubscriptionLimitDetails { PlanId = tenantSubscription.PlanId, Limit = limit, Utilized = utilized, Active = tenantSubscription.Active, ScheduledCancellationDate = tenantSubscription.ScheduledCancellationDate });
        }

        ///<summary>
        /// Gets the requested tenant id for a subscription id
        ///</summary>
        ///<param name="subscriptionId">The subscriptions unique identifier</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/subscriptions/{subscriptionId}/tenant-id")]
        public async Task<ApiResponse> GetTenantId(string subscriptionId, CancellationToken cancellationToken)
        {
            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionBySubscriptionId(subscriptionId, cancellationToken);
                if (tenantSubscription is null)
                {
                    throw new ApiException("No subscription was found for the provided subscription", StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve tenant id for subscription '{subscriptionId}'");
                throw new ApiException("Failed to retrieve tenant id for subscription", StatusCodes.Status500InternalServerError);
            }
            return new ApiResponse("Successfully retrieved tenant id", tenantSubscription.TenantId);
        }
    }
}