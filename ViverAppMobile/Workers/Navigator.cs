using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Views.General;

namespace ViverAppMobile.Workers
{
    internal static class Navigator
    {
        private static readonly Stack<Page> _navigationStack = new();
        private static readonly CancellationTokenSource transitionCts = new();

        private static bool _isTransitioning = false;
        public static bool IsTransitioning
        {
            get => _isTransitioning;
            set
            {
                _isTransitioning = value;
                if (!value)
                    transitionCts.Cancel();
            }
        }

        private static Page? currentPage;
        public static Page? CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;

                if (_navigationStack.Count > 0)
                    _navigationStack.Clear();

                if(value is not null)
                    _navigationStack.Push(value);
            }
        }

        public static bool CurrentPageIsLoginPage() => CurrentPage is LoginRegisterPage;

        public static async Task RedirectToMainPage()
        {
            if (CurrentPageIsLoginPage())
                return;

            try
            {
                await PopupHelper.PopAllPopUpAsync();
            }
            catch (Exception) { }

            MainThread.BeginInvokeOnMainThread(() => SwitchPage(new LoginRegisterPage()));

            try
            {
                await PopupHelper.PushLoadingAsync();

                if (Preferences.ContainsKey("user"))
                {
                    var user = UserHelper.GetLoggedUser();
                    await AuthSession.ClearAsync(user);
                }
            }
            catch (Exception) { }

            try
            {
                await PopupHelper.PopAllPopUpAsync();
            }
            catch (Exception) { }

            Master.CancelGlobalToken();
            
            WeakReferenceMessenger.Default.Send(new DesinstanceAllPages(true));
            UserHelper.RemoveLoggedUser();
        }

        public static void SwitchPage(Page page)
        {
            if (Application.Current?.Windows?.Count > 0)
            {
                CurrentPage = page;
                Application.Current.Windows[0].Page = page;
            }
        }

        public static void OpenFlyoutPage()
        {
            if (Application.Current.Windows[0].Page is FlyoutPage flyoutPage)
                flyoutPage.IsPresented = true;
        }

        public static void CloseFlyoutPage()
        {
            if (Application.Current.Windows[0].Page is FlyoutPage flyoutPage)
                flyoutPage.IsPresented = false;
        }

        public static void SwitchPatientPage(PatientPage patientPage) => WeakReferenceMessenger.Default.Send(new NavigateTabMessage(patientPage.ToString()));
        public static void SwitchAdminPage(AdminPage adminPage) => WeakReferenceMessenger.Default.Send(new NavigateTabIndex((int)adminPage));
        public static async Task PushOnlinePage(ScheduleDto roomSchedule)
        {
            ValueBunker<ScheduleDto>.SavedValue = roomSchedule;
            await PushNavigationAsync(new OnlinePage());
        }

        public static async Task PushNavigationAsync(Page newPage, uint duration = 250)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            try
            {
                var window = Application.Current?.Windows?.Count > 0 ? Application.Current.Windows[0] : null;
                if (window == null)
                {
                    _navigationStack.Push(newPage);
                    return;
                }

                var current = _navigationStack.Count > 0 ? _navigationStack.Peek() : null;
                if (current == null)
                {
                    _navigationStack.Push(newPage);
                    window.Page = newPage;
                    return;
                }

                var width = Math.Max(window.Width, 1);

                try
                {
                    await current.TranslateTo(-width, 0, duration, Easing.CubicInOut);
                }
                catch
                {
                    await current.FadeTo(0, duration);
                }

                newPage.TranslationX = width;
                _navigationStack.Push(newPage);

                window.Page = newPage;

                try
                {
                    await newPage.TranslateTo(0, 0, duration, Easing.CubicInOut);
                }
                catch
                {
                    await newPage.FadeTo(1, duration);
                }

                newPage.TranslationX = 0;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public static async Task<bool> PopNavigationAsync(uint duration = 250)
        {
            if (_isTransitioning) return false;
            if (_navigationStack.Count < 2) return false;

            _isTransitioning = true;
            try
            {
                var window = Application.Current?.Windows?.Count > 0 ? Application.Current.Windows[0] : null;
                if (window == null) return false;

                var current = _navigationStack.Pop();
                var previous = _navigationStack.Peek();

                var width = Math.Max(window.Width, 1);

                try
                {
                    await current.TranslateTo(width, 0, duration, Easing.CubicInOut);
                }
                catch
                {
                    await current.FadeTo(0, duration);
                }

                previous.TranslationX = -width;
                window.Page = previous;

                try
                {
                    await previous.TranslateTo(0, 0, duration, Easing.CubicInOut);
                }
                catch
                {
                    await previous.FadeTo(1, duration);
                }

                previous.TranslationX = 0;
                current.TranslationX = 0;

                return true;
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public static async Task WaitTransition()
        {
            if (!_isTransitioning)
                return;

            await Task.Run(() =>
            {
                var token = transitionCts.Token;

                while (!token.IsCancellationRequested && _isTransitioning)
                {
                    Task.Delay(100);
                }
            },Master.GlobalToken);
        }
    }
}
