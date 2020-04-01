using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ITenantsClient tenantsClient;
        private readonly SubscriptionsRepository subscriptionsRepo;
        private readonly ILogger<SubscriptionsController> logger;
        private readonly ChargeBeeOptions options;

        public SubscriptionsController(ITenantsClient tenantsClient, SubscriptionsRepository subscriptionsRepo, ILogger<SubscriptionsController> logger, ChargeBeeOptions options)
        {
            this.tenantsClient = tenantsClient;
            this.subscriptionsRepo = subscriptionsRepo;
            this.options = options;
            this.logger = logger;
        }

        [HttpGet("{domain}/subscriptions/checkout-existing-hosted-page-url")]
        public async Task<IActionResult> GetCheckoutExistingHostedPageUrl([FromRoute] string domain, [FromQuery] string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
            {
                var apiErrorContent = new ApiErrorContent();
                apiErrorContent.Errors.Add($"{nameof(planId)} was null or whitespace.");
                return BadRequest(apiErrorContent);
            }

            ContextTenant tenant = null;
            try
            {
                tenant = await this.tenantsClient.GetTenantAsync<ContextTenant>(domain);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve ContextTenant for domain '{domain}'.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByPgsqlDatabaseUsername(tenant.DatabaseUsername);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve SubscriptionId for PgsqlDatabaseUsername '{tenant.DatabaseUsername}'.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

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
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }

        [HttpGet("{domain}/subscriptions")]
        public async Task<IActionResult> GetSubscription([FromRoute] string domain)
        {
            ContextTenant tenant = null;
            try
            {
                tenant = await this.tenantsClient.GetTenantAsync<ContextTenant>(domain);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve ContextTenant for domain '{domain}'.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByPgsqlDatabaseUsername(tenant.DatabaseUsername);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve SubscriptionId for PgsqlDatabaseUsername '{tenant.DatabaseUsername}'.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(tenantSubscription);
        }

        [HttpGet("{domain}/subscriptions/limit-details")]
        public async Task<IActionResult> GetLimitDetails([FromRoute] string domain)
        {
            var apiErrorContent = new ApiErrorContent();

            ContextTenant tenant = null;
            try
            {
                tenant = await this.tenantsClient.GetTenantAsync<ContextTenant>(domain);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve ContextTenant for domain '{domain}'.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            TenantSubscription tenantSubscription = null;
            try
            {
                tenantSubscription = await this.subscriptionsRepo.GetTenantSubscriptionByPgsqlDatabaseUsername(tenant.DatabaseUsername);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve SubscriptionId for PgsqlDatabaseUsername '{tenant.DatabaseUsername}'.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            SubscriptionLimitDetails limitDetails = null;
            try
            {
                limitDetails = await ChargeBeeService.GetSubscriptLimitDetails(tenantSubscription.SubscriptionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to retrieve limit details from ChargeBee.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(limitDetails);
        }
    }
}