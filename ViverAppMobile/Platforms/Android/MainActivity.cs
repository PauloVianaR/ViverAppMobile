using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Extensions;
using Android.Net.Http;
using Android.OS;
using Android.Webkit;
using Firebase;
using Firebase.Messaging;
using Java.Lang.Annotation;
using Microsoft.Maui.Handlers;
using System.Collections.Concurrent;
using ViverAppMobile.Handlers;
using ViverAppMobile.Views.General;
using ViverAppMobile.Workers;
using WebView = Android.Webkit.WebView;

namespace ViverAppMobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            FirebaseApp.InitializeApp(this);

            Task.Run(async () =>
            {
                var token = await FirebaseMessaging.Instance.GetToken();
                System.Diagnostics.Debug.WriteLine($"[FCM TOKEN] {token}");
            });


            WebView.SetWebContentsDebuggingEnabled(true);

            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("AndroidPermissions", (handler, view) =>
            {
                try
                {
                    var webView = handler.PlatformView;
                    webView.Settings.JavaScriptEnabled = true;
                    webView.Settings.MediaPlaybackRequiresUserGesture = false;
                    webView.Settings.AllowFileAccess = true;
                    webView.Settings.AllowContentAccess = true;
                    webView.Settings.DomStorageEnabled = true;

                    webView.SetWebChromeClient(new CustomWebChromeClient());
                    webView.SetWebViewClient(new CustomWebViewClient());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao configurar WebView Android: {ex.Message}");
                }
            });
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        private void HandleIntent(Intent? intent)
        {
            if (intent?.Data != null)
            {
                var uri = intent.Data.ToString();

                if (uri.Contains("viverapp://pagamentosucesso"))
                {
                    MainThread.BeginInvokeOnMainThread(async() =>
                    {
                        await Navigator.PushNavigationAsync(new PaymentSuccessfulPage());
                    });
                }
            }
        }

        public class CustomWebChromeClient : WebChromeClient
        {
            public override void OnPermissionRequest(PermissionRequest? request)
            {
                request?.Grant(request.GetResources());
            }

            public override bool OnConsoleMessage(ConsoleMessage? consoleMessage)
            {
                Android.Util.Log.Info("WebViewConsole", $"{consoleMessage.Message()} (source: {consoleMessage.SourceId()}:{consoleMessage.LineNumber()})");
                return base.OnConsoleMessage(consoleMessage);
            }
        }

        public class CustomWebViewClient : WebViewClient
        {
            public override void OnReceivedSslError(WebView? view, SslErrorHandler? handler, SslError? error)
            {
                handler?.Proceed();
            }

            public override void OnPageFinished(WebView? view, string? url)
            {
                base.OnPageFinished(view, url);
                Android.Util.Log.Info("CustomWebViewClient", $"OnPageFinished: {url}");

                if(view is not null)
                    WebViewPageFinishTracker.TrySignal(view);
            }
        }

        public class WebViewPageFinishTracker
        {
            private static ConcurrentDictionary<int, TaskCompletionSource<bool>> map = new();

            public static void Register(WebView nativeWebView, TaskCompletionSource<bool> tcs)
            {
                map[nativeWebView.GetHashCode()] = tcs;
            }

            public static void Unregister(WebView nativeWebView)
            {
                map.TryRemove(nativeWebView.GetHashCode(), out _);
            }

            public static void TrySignal(Android.Webkit.WebView nativeWebView)
            {
                if (map.TryRemove(nativeWebView.GetHashCode(), out var tcs))
                    tcs.TrySetResult(true);
            }
        }
    }
}
