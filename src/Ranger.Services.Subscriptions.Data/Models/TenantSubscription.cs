using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.Services.Subscriptions.Data
{
    public class TenantSubscription
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string TenantId { get; set; }
        [Required]
        public string SubscriptionId { get; set; }
        [Required]
        public string CustomerId { get; set; }
        [Required]
        public string PlanId { get; set; }
        [Required]
        public bool Active { get; set; }
        public DateTime? ScheduledCancellationDate { get; set; }
        public DateTime OccurredAt { get; set; }
        [Required]
        public PlanLimits PlanLimits { get; set; }
    }
}