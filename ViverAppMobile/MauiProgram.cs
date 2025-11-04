using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Extensions.Configuration;
using Mopups.Hosting;
using ViverAppMobile.Controls;
using Microsoft.Maui.Handlers;
using CommunityToolkit.Maui;
using Syncfusion.Maui.Core.Hosting;
using LiveChartsCore.SkiaSharpView.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

#if ANDROID
using Android.Views;
#endif

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using Microsoft.Web.WebView2.Core;
#endif

namespace ViverAppMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDAxMTA1MkAzMzMwMmUzMDJlMzAzYjMzMzAzYlV1NDNKUXBYZ3VhNngwOHl2QkRrd1AzTVBZRWw3MFFVRkpqb043UGNudFk9");

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit(options => options.SetShouldEnableSnackbarOnWindows(true))
            .UseLiveCharts()
            .UseSkiaSharp()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
                fonts.AddFont("Roboto-Regular.ttf", "Roboto");
                fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
                fonts.AddFont("fontello.ttf", "AppIcons");
                fonts.AddFont("OpenSans-Semibold.ttf", "Semibold");
            })
            .ConfigureMopups();

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<FullWidthDatePicker, DatePickerHandler>();

#if WINDOWS
            DatePickerHandler.Mapper.AppendToMapping("FullWidth", (handler, view) =>
            {
                if (view is FullWidthDatePicker && handler.PlatformView is CalendarDatePicker dp)
                {
                    dp.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
                    dp.MinWidth = 0;
                }
            });
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var configBuilder = new ConfigurationBuilder();

        var assembly = typeof(App).Assembly;
        using var stream = assembly.GetManifestResourceStream("ViverAppMobile.appsettings.json") 
            ?? throw new FileNotFoundException("appsettings.json não encontrado como EmbeddedResource.");
        builder.Configuration.AddJsonStream(stream);

        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            events.AddWindows(windows =>
            {
                windows.OnLaunched((app, args) =>
                {
                    var window = MauiWinUIApplication.Current.Application.Windows[0].Handler.PlatformView;

                    IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                    AppWindow appWindow = AppWindow.GetFromWindowId(wndId);

                    if (appWindow.Presenter is OverlappedPresenter presenter)
                    {
                        presenter.SetBorderAndTitleBar(true, true);
                    }

                    appWindow.MoveAndResize(new RectInt32(100, 100, 412, 917));

                    var protocolArgs = args.Arguments;
                    if (!string.IsNullOrEmpty(protocolArgs))
                    {
                        App.HandleDeepLink(protocolArgs);
                    }
                });
            });
#endif
#if ANDROID
            events.AddAndroid(android => android.OnNewIntent((activity, intent) =>
            {
                var data = intent?.Data?.ToString();
                if (!string.IsNullOrEmpty(data))
                {
                    App.HandleDeepLink(data);
                }
            
            }));
#endif
        });

#if WINDOWS
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("WindowsMedia", (handler, view) =>
        {
            handler.PlatformView.NavigationCompleted += async (s, e) =>
            {
                try
                {
                    await Task.Delay(300);

                    var coreWebView2 = handler.PlatformView.CoreWebView2;
                    if (coreWebView2 == null)
                    {
                        await handler.PlatformView.EnsureCoreWebView2Async();
                        coreWebView2 = handler.PlatformView.CoreWebView2;
                    }

                    if (coreWebView2 != null)
                    {
                        coreWebView2.Settings.AreHostObjectsAllowed = true;
                        coreWebView2.Settings.IsWebMessageEnabled = true;

                        coreWebView2.PermissionRequested += (sender, args) =>
                        {
                            if (args.PermissionKind == CoreWebView2PermissionKind.Camera ||
                                args.PermissionKind == CoreWebView2PermissionKind.Microphone)
                            {
                                args.State = CoreWebView2PermissionState.Allow;
                            }
                        };

                        if(ViverAppMobile.Workers.Master.CanShowDevTools())
                            coreWebView2.OpenDevToolsWindow();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao inicializar WebView2: {ex}");
                }
            };
        });
#endif
        return builder.Build();
    }
}
