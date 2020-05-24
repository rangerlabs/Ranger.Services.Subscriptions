
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.Services.Subscriptions
{
    public class ResourceModel
    {
        [Required]
        public ResourceEnum Resource { get; set; }
    }
}