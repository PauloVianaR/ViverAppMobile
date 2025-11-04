using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using ViverApp.Shared.DTos;
using ViverAppMobile.Controls;
using ViverAppMobile.Handlers;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.General
{
    public partial class OnlinePageViewModel : ObservableObject, IViewModelInstancer
    {
        private bool videoActive = true;
        private bool audioActive = true;
        private ScheduleDto? currentAppointment;

        [ObservableProperty] private bool canConnectOrDisconnect = false;
        [ObservableProperty] private bool callStarted = false;
        [ObservableProperty] private string roomId = string.Empty;
        [ObservableProperty] private string videoIcon = "\ue821";
        [ObservableProperty] private string audioIcon = "\uf048";

        public async Task InitializeAsync()
        {
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            try
            {
                currentAppointment = ValueBunker<ScheduleDto>.SavedValue ?? throw new Exception("Nenhum atendimento selecionado!");
                await MainThread.InvokeOnMainThreadAsync(() => RoomId = currentAppointment.IdSchedule.ToString());

                await Task.CompletedTask;
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                CanConnectOrDisconnect = true;
            }
        }

        [RelayCommand]
        private async Task BackTo()
        {
            if (!CanConnectOrDisconnect)
                return;

            if (CallStarted && currentAppointment is not null)
                WeakReferenceMessenger.Default.Send(new OnlineConcluded(currentAppointment));

            await Navigator.PopNavigationAsync();
        }

        [RelayCommand]
        public async Task StartEndAsync(WebView webView)
        {
            if (CallStarted)
            {
                await BackTo();
                return;
            }

            if (!CanConnectOrDisconnect)
                return;
            CanConnectOrDisconnect = false;

            try
            {
                string hubUrl = Master.GetUrl(UrlType.VideoHubApi);
                if (string.IsNullOrWhiteSpace(hubUrl) || string.IsNullOrWhiteSpace(RoomId))
                    throw new Exception("Ocorreu uma falha ao tentar obter o link de conexão online. \nTente novamente mais tarde");

                if (DeviceInfo.Platform != DevicePlatform.WinUI)
                    await WebViewJsHandler.EnsureCameraAndMicPermissionsAsync();

                CallStarted = true;

                await Task.Delay(2000);

                int tries = 0;
                while (webView.Handler?.PlatformView == null && tries++ < 20)
                    await Task.Delay(100);

#if WINDOWS
                var nativeView = (Microsoft.UI.Xaml.Controls.WebView2?)webView.Handler?.PlatformView
                    ?? throw new Exception("Native WebView2 não disponível.");
#endif

                string js = $@"
                    (async function() {{
                        let retries = 0;
                        while (typeof window.init !== 'function' && retries < 50) {{
                            await new Promise(r => setTimeout(r, 100));
                            retries++;
                        }}
                        if (typeof window.init !== 'function') {{
                            console.error('❌ window.init ainda não está disponível após esperar.');
                            return 'init_not_ready';
                        }}
                        try {{
                            window.HUB_URL = '{hubUrl}';
                            console.log('🟢 Chamando init com RoomId = {RoomId}');
                            await window.init('{RoomId}');
                            console.log('✅ init executado com sucesso!');
                            return 'ok';
                        }} catch (ex) {{
                            console.error('❌ Erro ao executar init():', ex && ex.message ? ex.message : ex);
                            return 'init_failed';
                        }}
                    }})();";

                var tcsScriptExecuted = new TaskCompletionSource<string?>();

#if WINDOWS
                nativeView.CoreWebView2Initialized += (s, e) =>
                {
                    nativeView.CoreWebView2.DOMContentLoaded += async (s2, e2) =>
                    {
                        try
                        {
                            var op = nativeView.CoreWebView2.ExecuteScriptAsync(js);
                            string result = await op.AsTask();
                            tcsScriptExecuted.TrySetResult(result);                            
                        }
                        catch (Exception ex)
                        {
                            tcsScriptExecuted.TrySetResult(ex.Message);
                        }
                    };
                };
#endif
                var tcsNav = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                webView.Navigating += (_, args) =>
                {
                    Console.WriteLine($"Navigating --> Url= {args.Url}\nSource = {args.Source}");
                    tcsNav.TrySetResult(true);
                };
                webView.Navigated += (_, args) =>
                {
                    Console.WriteLine($"Navigated completed!\nUrl= {args.Url}");
                    tcsNav.TrySetResult(true);
                };
#if ANDROID
                int androidtries = 0;
                while (webView.Handler?.PlatformView == null && androidtries++ < 20)
                    await Task.Delay(100);

                var nativeView = webView.Handler?.PlatformView as Android.Webkit.WebView
                    ?? throw new Exception("nativeView Android não disponível.");

                var tcsPageFinished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                MainActivity.WebViewPageFinishTracker.Register(nativeView, tcsPageFinished);

                webView.Source = new UrlWebViewSource { Url = "file:///android_asset/WebControls/call.html" };

                var finished = await Task.WhenAny(tcsPageFinished.Task, Task.Delay(7000));
                if (finished != tcsPageFinished.Task)
                {
                    Debug.WriteLine("OnPageFinished timeout");
                }
                else
                {
                    await tcsPageFinished.Task;
                }
                MainActivity.WebViewPageFinishTracker.Unregister(nativeView);
#else
                webView.Source = "WebControls/call.html";
#endif
                var completedTask = await Task.WhenAny(tcsNav.Task, Task.Delay(5000));
                if (completedTask != tcsNav.Task)
                {
                    System.Diagnostics.Debug.WriteLine("Aviso: WebView não notificou Navigated em 5s — prosseguindo com fallback.");
                }
                else
                {
                    await tcsNav.Task;
                }

                if (DeviceInfo.Platform != DevicePlatform.WinUI)
                {
                    try
                    {
                        var injectionTask = webView.EvaluateJavaScriptAsync(js);
                        var completedTaskAndroid = await Task.WhenAny(injectionTask, Task.Delay(5000));
                        if (completedTaskAndroid == injectionTask)
                        {
                            var res = await injectionTask;
                            System.Diagnostics.Debug.WriteLine("Mobile injection result (MAUI): " + res);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Mobile injection (MAUI) timed out — tentando fallback nativo (Android).");
                            await WebViewJsHandler.ForceInjectJs(webView, js);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Mobile injection exception (MAUI): " + ex);
                        await WebViewJsHandler.ForceInjectJs(webView, js);
                    }
                }

#if WINDOWS
                try { await nativeView.EnsureCoreWebView2Async(); } catch { }

                var completed = await Task.WhenAny(tcsScriptExecuted.Task, Task.Delay(5000));
                if (completed != tcsScriptExecuted.Task)
                {
                    try
                    {
                        if(nativeView?.CoreWebView2?.Settings is null)
                            return;

                        nativeView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                        nativeView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;

                        nativeView.CoreWebView2.ServerCertificateErrorDetected += (sender, args) =>
                        {
                           args.Action = Microsoft.Web.WebView2.Core.CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
                        };

                        var op = nativeView.CoreWebView2.ExecuteScriptAsync(js);
                        string fallback = await op.AsTask();
                        System.Diagnostics.Debug.WriteLine("Fallback injection result: " + fallback);
                        
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Fallback injection exception: " + ex);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Initial injection result: " + tcsScriptExecuted.Task.Result);
                }
#endif
            }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message, "Ops...");
            }
            finally
            {
                CanConnectOrDisconnect = true;
            }
        }

        [RelayCommand]
        public async Task ToggleAudioAsync(WebView webView)
        {
            audioActive = !audioActive;
            AudioIcon = audioActive ? "\uf048" : "\uf047";

            await webView.EvaluateJavaScriptAsync("toggleAudio()");
        }

        [RelayCommand]
        public async Task ToggleVideoAsync(WebView webView)
        {
            videoActive = !videoActive;
            VideoIcon = videoActive ? "\ue821" : "\ue822";

            await webView.EvaluateJavaScriptAsync("toggleVideo()");
        }

        [RelayCommand]
        public async Task Reconnect(WebView webView)
        {
            if (!CanConnectOrDisconnect)
                return;

            if (!await Messenger.ShowQuestionMessage("Deseja tentar se reconectar?", "Reconexão"))
                return;

            webView.Source = "WebControls/loading.html";
            CallStarted = false;

            await StartEndAsync(webView);
        }
    }
}
