using CommunityToolkit.Mvvm.Messaging;
using ViverApp.Shared.Models;
using ViverAppMobile.Helpers;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Admin;
using ViverAppMobile.Views.Doctor;
using ViverAppMobile.Views.General;
using ViverAppMobile.Views.Manager;
using ViverAppMobile.Views.Patient;
using ViverAppMobile.Workers;

namespace ViverAppMobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[XAML Exception] {e.Exception}");
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                Page window;

                if (Preferences.ContainsKey("user"))
                {
                    var user = UserHelper.GetLoggedUser() ?? throw new Exception();
                    window = user.Usertype switch
                    {
                        (int)UserType.Admin => new AdminMainPage(),
                        (int)UserType.Patient => new PatientMainPage(),
                        (int)UserType.Manager => new ManagerMainPage(),
                        (int)UserType.Doctor => new DoctorMainPage(),
                        _ => throw new Exception()
                    };
                }
                else
                {
                    throw new Exception();
                }

                Navigator.CurrentPage = window;
                return new Window(window);
            }
            catch (Exception)
            {
                LoginRegisterPage registerpage = new();
                Navigator.CurrentPage = registerpage;
                return new Window(registerpage);
            }
        }

        public static void HandleDeepLink(string url)
        {
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                if (url.StartsWith("viverapp://pagamentosucesso"))
                {
                    Navigator.SwitchPage(new PatientMainPage());
                    Navigator.SwitchPatientPage(Models.PatientPage.Payment);
                }
            });
        }
    }
}