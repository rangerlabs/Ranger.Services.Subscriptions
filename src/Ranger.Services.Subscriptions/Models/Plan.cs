namespace Ranger.Services.Subscriptions
{
    public class RangerPlan
    {
        public RangerPlan(string id, LimitFields limitFields)
        {
            this.Id = id;
            this.LimitFields = limitFields;

        }
        public string Id { get; set; }
        public LimitFields LimitFields { get; set; }
    }
}