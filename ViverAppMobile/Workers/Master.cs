using Microsoft.Extensions.Configuration;
using System.Globalization;
using ViverApp.Shared.Models;

namespace ViverAppMobile.Workers
{
    internal static class Master
    {
        private static CancellationTokenSource GlobalTokenSource = new();
        public static CancellationToken GlobalToken { get; set; } = GlobalTokenSource.Token;
        public static CultureInfo Culture { get; set; } = new("pt-BR");
        public static bool WasUnauthorized { get; set; } = false;
        public static AppMode AppMode { get; set; } = AppMode.Homologation;
        
        public static string GetUrl(UrlType type)
        {
            var config = IPlatformApplication.Current.Services.GetService<IConfiguration>();
            if (config == null)
                return string.Empty;

            var apiSection = config.GetSection("ApiSettings");
            if (!apiSection.Exists())
                return string.Empty;

            switch (type)
            {
                case UrlType.DataBaseApi:
                    bool useLocalhost = bool.TryParse(apiSection["UseLocalhost"], out var result) && result;

                    if (useLocalhost)
                        return apiSection["LocalhostUrl"] ?? string.Empty;

                    return apiSection["ProductionUrl"] ?? string.Empty;
                case UrlType.VideoHubApi:
                    return apiSection["HubUrl"] ?? string.Empty;
                case UrlType.PagBankWebhook:
                    return apiSection["PagBankWebhook"] ?? string.Empty;
                case UrlType.PagBankRedirect:
                    return apiSection["PagBankRedirectUrl"] ?? string.Empty;
            }
            return string.Empty;
        }

        public static bool CanShowDevTools()
        {
            var config = IPlatformApplication.Current.Services.GetService<IConfiguration>();
            if (config == null)
                return false;

            var apiSection = config.GetSection("ApiSettings");
            if (!apiSection.Exists())
                return false;

            return bool.TryParse(apiSection["ShowDevTools"], out var result) && result;
        }

        public static void CancelGlobalToken()
        {
            GlobalTokenSource?.Cancel();
        }

        public static void ResetGlobalToken()
        {
            GlobalTokenSource = new();
            GlobalToken = GlobalTokenSource.Token;
            WasUnauthorized = false;
        }
    }

    internal enum UrlType
    {
        DataBaseApi,
        VideoHubApi,
        PagBankWebhook,
        PagBankRedirect
    }
}
