using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using ViverAppMobile.ViewModels;
using ViverAppMobile.Views;

namespace ViverAppMobile.Views.Patient
{
    public partial class PatientTabbedPage : ContentPage
    {
        public PatientTabbedPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
