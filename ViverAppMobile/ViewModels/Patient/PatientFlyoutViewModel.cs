using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Patient
{
    public partial class PatientFlyoutViewModel : ObservableObject
    {
        [ObservableProperty] private bool isNotUserPremium = false;
        public PatientFlyoutViewModel()
        {
            var user = UserHelper.GetLoggedUser();
            if(user is not null)
            {
                IsNotUserPremium = user.IsPremium != (sbyte)1;
            }
        }

        [RelayCommand] private void ShowHomeView() => Navigator.SwitchPatientPage(PatientPage.Home);
        [RelayCommand] private void ShowScheduleView() => Navigator.SwitchPatientPage(PatientPage.Schedule);
        [RelayCommand] private void ShowAgendaView() => Navigator.SwitchPatientPage(PatientPage.Agenda);
        [RelayCommand] private void ShowPaymentView() => Navigator.SwitchPatientPage(PatientPage.Payment);
        [RelayCommand] private void ShowProfileView() => WeakReferenceMessenger.Default.Send(new ShowProfilePageSelectTabMessage("1"));
        [RelayCommand] private void ShowPremiumPlans() => WeakReferenceMessenger.Default.Send(new ShowProfilePageSelectTabMessage("2"));
    }
}
