using Ranger.RabbitMQ;

namespace Ranger.Services.Subscriptions
{
    [MessageNamespace("subscriptions")]
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