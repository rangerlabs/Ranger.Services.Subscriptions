using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ranger.Services.Subscriptions.Data
{
    public class PlanLimits
    {
        public int Id { get; set; }
        public int TenantSubscriptionId { get; set; }
        public TenantSubscription TenantSubscription { get; set; }
        [Required]
        public int Geofences { get; set; }
        [Required]
        public int Integrations { get; set; }
        [Required]
        public int Projects { get; set; }
        [Required]
        public int Accounts { get; set; }
    }
}