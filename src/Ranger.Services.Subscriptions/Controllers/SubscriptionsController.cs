using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChargeBee.Api;
using ChargeBee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.Services.Subscriptions;

[ApiController]
public class SubscriptionsController : ControllerBase
{
    private readonly ILogger<SubscriptionsController> logger;
    private readonly ChargeBeeOptions options;
    public SubscriptionsController(ILogger<SubscriptionsController> logger, ChargeBeeOptions options)
    {
        this.options = options;
        this.logger = logger;
    }

    [HttpGet("{domain}/subscriptions/checkout-existing-hosted-page-url")]
    public async Task<IActionResult> GetCheckoutExistingHostedPageUrl([FromRoute] string domain)
    {
        // if (string.IsNullOrWhiteSpace(email))
        // {
        //     var apiErrorContent = new ApiErrorContent();
        //     apiErrorContent.Errors.Add($"{nameof(email)} was null or whitespace.");
        //     return BadRequest(apiErrorContent);
        // }

        //get from the database
        String subscriptionId = "AzqgwaRudbEVbEZG";
        RangerChargeBeeHostedPage result = null;
        try
        {
            var _ = HostedPage.CheckoutExisting()
                    .SubscriptionId(subscriptionId)
                    .SubscriptionPlanId("startup")
                    .Request()
                    .HostedPage;
            result = new RangerChargeBeeHostedPage
            {
                Id = _.Id,
                Type = _.HostedPageType,
                Url = _.Url,
                State = _.State,
                Embed = _.Embed,
                CreatedAt = _.CreatedAt,
                ExpiresAt = _.ExpiresAt,
                UpdatedAt = _.UpdatedAt,
                ResourceVersion = _.ResourceVersion
            };


        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to get the hosted page for Subscription Id '{subscriptionId}'.");
        }
        return Ok(result);
    }
}