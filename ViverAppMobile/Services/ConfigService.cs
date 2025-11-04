using ViverApp.Shared.Models;

namespace ViverAppMobile.Services
{
    public class ConfigService : Service<Config>
    {
        public ConfigService() : base($"{baseApiPoint}/Config") { }
    }
}
