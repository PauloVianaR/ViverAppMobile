using System.Text.Json;
using ViverAppMobile.Handlers;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Services
{
    public class BaseService
    {
        protected HttpClient HttpClient { get; } = null!;
        private static readonly HttpClient _sharedHttpClient = null!;
        protected static readonly string baseUrl = Master.GetUrl(UrlType.DataBaseApi);
        protected const string baseApiPoint = "api/v1";        
        protected static JsonSerializerOptions? _jsonOptions;

        protected BaseService()
        {
            HttpClient = _sharedHttpClient;
        }

        static BaseService()
        {
            var handler = new JwtHandler();
            _sharedHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        protected BaseService(string apiUri)
        {
            HttpClient = new()
            {
                BaseAddress = new Uri(apiUri)
            };
        }
    }
}
