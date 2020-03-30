using System;

namespace Ranger.Services.Subscriptions
{
    public class RangerChargeBeeHostedPage
    {
        public string Id { get; set; }
        public ChargeBee.Models.HostedPage.TypeEnum? Type { get; set; }
        public string Url { get; set; }
        public ChargeBee.Models.HostedPage.StateEnum? State { get; set; }
        public bool Embed { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? ResourceVersion { get; set; }
    }
}