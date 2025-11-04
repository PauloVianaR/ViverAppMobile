using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ViverApp.Shared.DTos;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Views.Manager;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Manager
{
    public partial class ManagerMainPageViewModel : ObservableObject
    {
        private ManagerHomeView? _homeview;
        private ManagerAgendaView? _agendaView;
        private ManagerHistoricView? _historicView;
        private ManagerProfileView? _profileView;

        [ObservableProperty] private int tabIndex = 0;
        [ObservableProperty] private View currentView;
        [ObservableProperty] private UserDto? loggedUser;

        public ManagerMainPageViewModel()
        {
            _homeview = new();
            CurrentView = _homeview;
            LoggedUser = UserHelper.GetLoggedUser();

            WeakReferenceMessenger.Default.Register<NavigateTabIndex>(this, (r, m) => this.SelectedTab(m.Value));
            WeakReferenceMessenger.Default.Register<DesinstanceAllPages>(this, (r, m) => this.DesinstanceAll());
            WeakReferenceMessenger.Default.Register<DesinstancePagesExceptOneMessage>(this, (r, m) => this.DesinstanceAllExecpt(m.Value));
        }

        [RelayCommand]
        private void SelectedTab(int index)
        {
            TabIndex = index;
            this.InstanceView((ManagerPage)index);
        }

        [RelayCommand] private async Task Loggout() => await Navigator.RedirectToMainPage();

        private void InstanceView(ManagerPage page)
        {
            switch (page)
            {
                case ManagerPage.Home:
                    _homeview ??= new();
                    CurrentView = _homeview;
                    break;

                case ManagerPage.Agenda:
                    _agendaView ??= new();
                    CurrentView = _agendaView;
                    break;

                case ManagerPage.Historic:
                    _historicView ??= new();
                    CurrentView = _historicView;
                    break;

                case ManagerPage.Profile:
                    _profileView ??= new();
                    CurrentView = _profileView;
                    break;

                default:
                    _homeview ??= new();
                    CurrentView = _homeview;
                    break;
            }
        }

        private void DesinstanceAllExecpt(string pageExecpt)
        {
            _homeview = pageExecpt == ManagerPage.Home.ToString() ? _homeview : null;
            _agendaView = pageExecpt == ManagerPage.Agenda.ToString() ? _agendaView : null;
            _historicView = pageExecpt == ManagerPage.Historic.ToString() ? _historicView : null;
            _profileView = pageExecpt == ManagerPage.Profile.ToString() ? _profileView : null;
        }

        private void DesinstanceAll()
        {
            _agendaView = null;
            _historicView = null;
            _homeview = null;
            _profileView = null;
        }
    }
}
