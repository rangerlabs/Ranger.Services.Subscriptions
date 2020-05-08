using Ranger.Services.Subscriptions.Data;

namespace Ranger.Services.Subscriptions
{
    public class RangerPlan
    {
        public RangerPlan(string id, PlanLimits planLimits)
        {
            this.Id = id;
            this.PlanLimits = planLimits;

        }
        public string Id { get; set; }
        public PlanLimits PlanLimits { get; set; }
    }
}