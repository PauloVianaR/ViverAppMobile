using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Views.Doctor;
using ViverAppMobile.Views.General;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Doctor
{
    public partial class DoctorMainPageViewModel : ObservableObject
    {
        private DoctorAgendaView? _agendaView;
        private DoctorHistoricView? _historicView;
        private DoctorHomeView? _homeview;
        private DoctorProfileView? _profileView;

        [ObservableProperty] private int tabIndex = 0;
        [ObservableProperty] private View currentView;
        [ObservableProperty] private UserDto? loggedUser;

        public DoctorMainPageViewModel()
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
            this.InstanceView((DoctorPage)index);
        }

        [RelayCommand] private async Task Loggout() => await Navigator.RedirectToMainPage();

        private void InstanceView(DoctorPage page)
        {
            switch (page)
            {
                case DoctorPage.Home:
                    _homeview ??= new();
                    CurrentView = _homeview;
                    break;

                case DoctorPage.Agenda:
                    _agendaView ??= new();
                    CurrentView = _agendaView;
                    break;

                case DoctorPage.Historic:
                    _historicView ??= new();
                    CurrentView = _historicView;
                    break;

                case DoctorPage.Profile:
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
            _homeview = pageExecpt == DoctorPage.Home.ToString() ? _homeview : null;
            _agendaView = pageExecpt == DoctorPage.Agenda.ToString() ? _agendaView : null;
            _historicView = pageExecpt == DoctorPage.Historic.ToString() ? _historicView : null;
            _profileView = pageExecpt == DoctorPage.Profile.ToString() ? _profileView : null;
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
