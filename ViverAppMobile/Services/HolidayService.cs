using ViverApp.Shared.Models;

namespace ViverAppMobile.Services
{
    public class HolidayService : Service<Holiday>
    {
        public HolidayService() : base($"{baseApiPoint}/Holiday") { }
    }
}
