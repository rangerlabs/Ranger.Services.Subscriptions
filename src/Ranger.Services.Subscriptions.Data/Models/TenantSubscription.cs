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
        public string PlanId { get; set; }

    }
}