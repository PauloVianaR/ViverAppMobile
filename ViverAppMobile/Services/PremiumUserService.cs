using ViverApp.Shared.Models;

namespace ViverAppMobile.Services
{
    public class PremiumUserService : Service<PremiumUser>
    {
        public PremiumUserService() : base($"{baseApiPoint}/PremiumUser") { }
    }
}
