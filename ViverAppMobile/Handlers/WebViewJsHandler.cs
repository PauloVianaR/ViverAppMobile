using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Handlers;

internal class WebViewJsHandler
{
    public static async Task ForceInjectJs(WebView webView, string js)
    {
#if ANDROID
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        using (var native = webView.Handler?.PlatformView as Android.Webkit.WebView)
        {
            if (native != null)
            {
                native.Post(() =>
                {
                    try
                    {
                        native.EvaluateJavascript(js, new ValueCallbackImpl(tcs));
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });

                var completed = await Task.WhenAny(tcs.Task, Task.Delay(3000));
                if (completed == tcs.Task)
                {
                    System.Diagnostics.Debug.WriteLine("Fallback Android EvaluateJavascript result: " + tcs.Task.Result);
                }
            }
        }
#endif
        await Task.CompletedTask;
    }

    public static async Task EnsureCameraAndMicPermissionsAsync()
    {
        var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
        var micStatus = await Permissions.RequestAsync<Permissions.Microphone>();

        if (cameraStatus != PermissionStatus.Granted || micStatus != PermissionStatus.Granted)
            throw new Exception("Permissões de câmera e microfone são necessárias.");
    }
}

#if ANDROID
public class ValueCallbackImpl(TaskCompletionSource<string?> tcs) : Java.Lang.Object, Android.Webkit.IValueCallback
{
    private readonly TaskCompletionSource<string?> _tcs = tcs;

    public void OnReceiveValue(Java.Lang.Object? value)
    {
        _tcs.TrySetResult(value?.ToString());
    }
}
#endif
