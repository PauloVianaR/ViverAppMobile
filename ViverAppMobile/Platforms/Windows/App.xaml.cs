using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using ViverAppMobile.Views.General;
using ViverAppMobile.Workers;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ViverAppMobile.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var mainInstance = AppInstance.FindOrRegisterForKey("main");

            mainInstance.Activated += (sender, activatedArgs) =>
            {
                if (activatedArgs.Kind == ExtendedActivationKind.Protocol)
                {
                    var protocolArgs = (ProtocolActivatedEventArgs)activatedArgs.Data;
                    var uri = protocolArgs.Uri?.ToString();

                    if (!string.IsNullOrEmpty(uri))
                        HandleProtocol(uri);
                }
            };
        }

        private void HandleProtocol(string uri)
        {
            if (uri.Contains("viverapp://pagamentosucesso"))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Navigator.PushNavigationAsync(new PaymentSuccessfulPage());
                });
            }
        }
    }
}
