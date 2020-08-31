using System.Threading.Tasks;
using Ranger.RabbitMQ;
using Shouldly;
using Xunit;

namespace Ranger.Services.Subscriptions.Tests
{
    public class HandlerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IBusSubscriber busSubscriber;
        private readonly CustomWebApplicationFactory _factory;

        public HandlerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            this.busPublisher = factory.Services.GetService(typeof(IBusPublisher)) as IBusPublisher;
            this.busSubscriber = factory.Services.GetService(typeof(IBusSubscriber)) as IBusSubscriber;
        }

        [Fact]
        public void Subscriptions_Starts()
        { }
    }
}