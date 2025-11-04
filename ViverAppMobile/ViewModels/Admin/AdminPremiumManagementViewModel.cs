using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverAppMobile.Controls;
using ViverAppMobile.Models;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Admin
{
    public partial class AdminPremiumManagementViewModel : ObservableObject, IViewModelInstancer
    {
        public AdminPremiumManagementViewModel()
        {
            
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task<string?> LoadAllAsync()
        {
            return (Task<string?>)Task.CompletedTask;
        }

        [RelayCommand] private async Task BackToMainPage()
        {
            await Navigator.PopNavigationAsync();
        }
    }
}
