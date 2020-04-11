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
        private readonly ILogger<IncrementResourceCountHandler> logger;
        private readonly SubscriptionsService subscriptionsService;

        public IncrementResourceCountHandler(IBusPublisher busPublisher, SubscriptionsService subscriptionsService, ILogger<IncrementResourceCountHandler> logger)
        {
            this.subscriptionsService = subscriptionsService;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(IncrementResourceCount message, ICorrelationContext context)
        {
            var newCount = await subscriptionsService.IncrementResource(message.TenantId, message.Resource);
            busPublisher.Publish(new ResourceCountIncremented(message.Resource, newCount), context);
        }
    }
}