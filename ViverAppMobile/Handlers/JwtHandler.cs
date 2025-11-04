using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ViverApp.Shared.DTos;
using Polly;
using Polly.Retry;
using ViverAppMobile.Models;
using ViverAppMobile.Workers;
using ViverApp.Shared.Models;

namespace ViverAppMobile.Handlers;

public partial class JwtHandler(HttpMessageHandler inner = null!) : DelegatingHandler(inner ?? new HttpClientHandler()
{ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator})
{
    private readonly string _baseApiUrl = Master.GetUrl(UrlType.DataBaseApi);

    private static readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.RequestTimeout ||
                           r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                           r.StatusCode == HttpStatusCode.GatewayTimeout)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
                onRetry: (outcome, delay, attempt, ctx) =>
                {
                    Console.WriteLine($"[Retry {attempt}] Nova tentativa em {delay.TotalSeconds}s...");
                });

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await AuthSession.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _retryPolicy.ExecuteAsync(
            async ct => await base.SendAsync(request, ct),
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            await HttpStatusCodeValidation(response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken), cancellationToken);
        }
        else
        {
            var refreshed = await TryRefreshToken(cancellationToken);

            if (refreshed)
            {
                var newToken = await AuthSession.GetAccessTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                response = await _retryPolicy.ExecuteAsync(
                    async ct => await base.SendAsync(request, ct),
                    cancellationToken);

                await HttpStatusCodeValidation(response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken), cancellationToken);
            }
            else
            {
                await HttpStatusCodeValidation(HttpStatusCode.Unauthorized, "Sua senha foi expirada. Faça o login novamente", cancellationToken);
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshToken(CancellationToken cancel)
    {
        if (cancel.IsCancellationRequested)
            return false;

        var refreshToken = await AuthSession.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        if (!Preferences.ContainsKey("usertype"))
            return false;

        int usertype = Preferences.Get("usertype", 0);
        if (usertype == 0)
            return false;

        using var client = new HttpClient() { BaseAddress = new Uri(_baseApiUrl) };

        var request = new RefreshRequestDto { RefreshToken = refreshToken, UserType = usertype };
        var response = await client.PostAsJsonAsync("api/v1/Auth/refresh", request, cancellationToken: cancel);

        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadAsStringAsync(cancel);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("access_token", out var at) &&
            root.TryGetProperty("refresh_token", out var rt))
        {
            var access = at.GetString() ?? "";
            var refresh = rt.GetString() ?? "";
            if (!string.IsNullOrEmpty(access) && !string.IsNullOrEmpty(refresh))
            {
                await AuthSession.SaveTokensAsync(access, refresh, usertype);
                return true;
            }
        }

        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancel);
        if (tokens is not null &&
            !string.IsNullOrEmpty(tokens.AccessToken) &&
            !string.IsNullOrEmpty(tokens.RefreshToken))
        {
            await AuthSession.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken, usertype);
            return true;
        }

        return false;
    }

    public static async Task HttpStatusCodeValidation(HttpStatusCode code, string content, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        List<HttpStatusCode> codesBlackList =
        [
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.TooManyRequests
        ];

        if (codesBlackList.Contains(code))
        {
            Master.WasUnauthorized = true;

            string contentMsg = content;

            if (contentMsg.Contains("message"))
                contentMsg =
                    contentMsg.Split("message")
                    .GetValue(1)
                    .ToString()
                    .Replace(":", string.Empty)
                    .Replace("\"", string.Empty)
                    .Replace("}", string.Empty)
                    .Trim();

            if (!content.Contains("Senha") && !Navigator.CurrentPageIsLoginPage())
                contentMsg += "\n\nVocê será redirecionado para a tela de login";

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                string msg = $"Ops: {contentMsg}";
                if (Master.AppMode == AppMode.Production)
                    msg = $"[{(int)code}] {code}\n\nOps: {contentMsg}";

                await Messenger.ShowErrorMessageAsync(msg);
                await Navigator.RedirectToMainPage();
            });
        }
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
