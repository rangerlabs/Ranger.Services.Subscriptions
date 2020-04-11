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
    public class DecrementResourceCountHandler : ICommandHandler<DecrementResourceCount>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<IncrementResourceCountHandler> logger;
        private readonly SubscriptionsService subscriptionsService;

        public DecrementResourceCountHandler(IBusPublisher busPublisher, SubscriptionsService subscriptionsService, ILogger<IncrementResourceCountHandler> logger)
        {
            this.subscriptionsService = subscriptionsService;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        public async Task HandleAsync(DecrementResourceCount message, ICorrelationContext context)
        {

            var newCount = await subscriptionsService.DecrementResource(message.TenantId, message.Resource);
            busPublisher.Publish(new ResourceCountDecremented(message.Resource, newCount), context);
        }
    }
}