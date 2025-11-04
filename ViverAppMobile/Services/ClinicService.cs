using ViverApp.Shared.Models;

namespace ViverAppMobile.Services
{
    public class ClinicService : Service<Clinic>
    {
        public ClinicService() : base($"{baseApiPoint}/Clinic") { }
    }
}
