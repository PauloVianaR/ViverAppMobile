using ViverApp.Shared.Models;

namespace ViverAppMobile.Services
{
    public class PremiumPlanService : Service<PremiumPlan>
    {
        public PremiumPlanService() : base($"{baseApiPoint}/PremiumPlan") { }
    }
}
