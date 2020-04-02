using System.ComponentModel.DataAnnotations;

namespace Ranger.Services.Subscriptions.Data
{
    public class UtilizationDetails
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int TenantSubscriptionId { get; set; }
        [Required]
        public TenantSubscription TenantSubscription { get; set; }
        [Required]
        public int GeofenceCount { get; set; }
        [Required]
        public int ProjectCount { get; set; }
        [Required]
        public int IntegrationCount { get; set; }
        [Required]
        public int AccountCount { get; set; }
    }
}