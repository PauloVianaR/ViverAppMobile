using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Views.General;
using ViverAppMobile.Views.Patient;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Patient
{
    public partial class PatientMainPageViewModel : ObservableObject
    {
        private PatientHomeView? _homeView;
        private PatientScheduleView? _scheduleView;
        private PatientAgendaView? _agendaView;
        private PaymentView? _paymentView;
        private PatientProfileView? _profileView;

        [ObservableProperty] private View? currentView;
        [ObservableProperty] private int selectedIndex;
        [ObservableProperty] private string userName;
        [ObservableProperty] private bool isUserPremium;

        public PatientMainPageViewModel()
        {
            _homeView = new();
            CurrentView = _homeView;
            SelectedIndex = 0;
            UserName = "usuário";
            IsUserPremium = false;

            WeakReferenceMessenger.Default.Register<NavigateTabMessage>(this, (r, m) => InstanceView(m.Value));
            WeakReferenceMessenger.Default.Register<DesinstancePagesExceptOneMessage>(this, (r, m) => this.DesinstanceAllExecpt(m.Value));
            WeakReferenceMessenger.Default.Register<DesinstanceAllPages>(this, (r, m) => this.DesinstanceAll());
            WeakReferenceMessenger.Default.Register<UserChangedMessage>(this, (r, m) => LoadUser(m.Value));
            WeakReferenceMessenger.Default.Register<ShowSchedulePageSelectAppointmentMessage>(this, (r, m) =>
            {
                InstanceView(PatientPage.Schedule.ToString());
                WeakReferenceMessenger.Default.Send(new ShowSchedulePageSelectAppointmentByMainMessage(m.Value));
            });
            WeakReferenceMessenger.Default.Register<ShowProfilePageSelectTabMessage>(this, (r, m) =>
            {
                InstanceView(PatientPage.Profile.ToString());
                WeakReferenceMessenger.Default.Send(new ShowProfilePageSelectTabByMainMessage(m.Value));
            });
            WeakReferenceMessenger.Default.Register<ShowPaymentPageSelectScheduleToPayMessage>(this, (r, m) =>
            {
                InstanceView(PatientPage.Payment.ToString());
                WeakReferenceMessenger.Default.Send(new ShowPaymentPageSelectScheduleToPayByMainMessage(m.Value));
            });

            LoadUser(true);
        }

        private void LoadUser(bool userChanged = false)
        {
            if (!userChanged)
                return;

            try
            {
                if (!Preferences.ContainsKey("user"))
                    throw new Exception();

                var user = UserHelper.GetLoggedUser() ?? throw new Exception();
                UserName = user.Name ?? "usuário";
                IsUserPremium = user.IsPremium == (sbyte)1;
            }
            catch(Exception)
            {
                Messenger.ShowErrorMessage($"Não foi possível carregar as informações do usuário \nErro:O aplicativo não conseguiu carregar o usuário salvo na memória.\nTente fazer o login novamente.");
                _ = Navigator.RedirectToMainPage();
            }
        }

        private void InstanceView(string viewName)
        {
            switch (viewName)
            {
                case "Home":
                    _homeView ??= new();
                    CurrentView = _homeView;
                    SelectedIndex = 0;
                    break;

                case "Schedule":
                    _scheduleView ??= new();
                    CurrentView = _scheduleView;
                    SelectedIndex = 1;
                    break;

                case "Agenda":
                    _agendaView ??= new();
                    CurrentView = _agendaView;
                    SelectedIndex = 2;
                    break;

                case "Payment":
                    _paymentView ??= new();
                    CurrentView = _paymentView;
                    SelectedIndex = 3;
                    break;

                case "Profile":
                    _profileView ??= new();
                    CurrentView = _profileView;
                    SelectedIndex = 4;
                    break;
            }

            Navigator.CloseFlyoutPage();
        }
        [RelayCommand] private async Task Loggout()
        {
            await Navigator.RedirectToMainPage();
        }

        [RelayCommand]
        private void SelectedTab(int index)
        {
            SelectedIndex = index;
            Navigator.SwitchPatientPage((PatientPage)index);
        }

        private void DesinstanceAllExecpt(string pageExecpt)
        {
            _homeView = pageExecpt == PatientPage.Home.ToString() ? _homeView : null;
            _scheduleView = pageExecpt == PatientPage.Schedule.ToString() ? _scheduleView : null;
            _agendaView = pageExecpt == PatientPage.Agenda.ToString() ? _agendaView : null;
            _paymentView = pageExecpt == PatientPage.Payment.ToString() ? _paymentView : null;
            _profileView = pageExecpt == PatientPage.Profile.ToString() ? _profileView : null;
        }

        private void DesinstanceAll()
        {
            _homeView = null;
            _scheduleView = null;
            _agendaView = null;
            _paymentView = null;
            _profileView = null;
        }
    }
}
