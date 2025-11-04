using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverAppMobile.Models;
using ViverAppMobile.Views.Admin;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Admin
{
    public partial class AdminMainPageViewModel : ObservableObject
    {
        private AdminHomeView? _homeview;
        private AdminClinicView? _clinicview;
        private AdminAppointmentsManagementView? _appointmentsmanagementview;
        private AdminAnalyticsView? _analyticsview;
        private AdminNotificationView? _notificationview;
        private AdminUserManagementView? _usermanagementview;

        [ObservableProperty] private int tabIndex = 0;
        [ObservableProperty] private View currentView;

        public AdminMainPageViewModel()
        {
            _homeview = new();
            CurrentView = _homeview;

            WeakReferenceMessenger.Default.Register<NavigateTabIndex>(this, (r, m) => this.SelectedTab(m.Value));
            WeakReferenceMessenger.Default.Register<DesinstanceAllPages>(this, (r,m) => this.DesinstanceAll());
            WeakReferenceMessenger.Default.Register<DesinstancePagesExceptOneMessage>(this, (r, m) => this.DesinstanceAllExecpt(m.Value));
        }

        [RelayCommand] private void SelectedTab(int index)
        {
            TabIndex = index;
            this.InstanceView((AdminPage)index);
        }

        [RelayCommand] private async Task Loggout() => await Navigator.RedirectToMainPage();

        private void InstanceView(AdminPage page)
        {
            switch (page)
            {
                case AdminPage.Home:
                    _homeview ??= new();
                    CurrentView = _homeview;
                    break;

                case AdminPage.Clinic:
                    _clinicview ??= new();
                    CurrentView = _clinicview;
                    break;

                case AdminPage.Appointments:
                    _appointmentsmanagementview ??= new();
                    CurrentView = _appointmentsmanagementview;
                    break;

                case AdminPage.Analytics:
                    _analyticsview ??= new();
                    CurrentView = _analyticsview;
                    break;

                case AdminPage.Notification:
                    _notificationview ??= new();
                    CurrentView = _notificationview;
                    break;

                case AdminPage.UserManagement:
                    _usermanagementview ??= new();
                    CurrentView = _usermanagementview;
                    break;

                default:
                    _homeview ??= new();
                    CurrentView = _homeview;
                    break;
            }
        }

        private void DesinstanceAllExecpt(string pageExecpt)
        {
            _homeview = pageExecpt == AdminPage.Home.ToString() ? _homeview : null;
            _clinicview = pageExecpt == AdminPage.Clinic.ToString() ? _clinicview : null;
            _appointmentsmanagementview = pageExecpt == AdminPage.Appointments.ToString() ? _appointmentsmanagementview : null;
            _analyticsview = pageExecpt == AdminPage.Analytics.ToString() ? _analyticsview : null;
            _notificationview = pageExecpt == AdminPage.Notification.ToString() ? _notificationview : null;
            _usermanagementview = pageExecpt == AdminPage.UserManagement.ToString() ? _usermanagementview : null;
        }

        private void DesinstanceAll()
        {
            _homeview = null;
            _clinicview = null;
            _appointmentsmanagementview = null;
            _analyticsview = null;
            _notificationview = null;
            _usermanagementview = null;
        }
    }
}
