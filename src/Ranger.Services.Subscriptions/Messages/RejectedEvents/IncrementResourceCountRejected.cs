using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    public class IncrementResourceCountRejected : IRejectedEvent
    {
        public IncrementResourceCountRejected(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }

        public string Reason { get; }
        public string Code { get; }
    }
}