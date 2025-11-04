using ViverAppMobile.Controls;

namespace ViverAppMobile.Views.Admin;

public partial class AdminPremiumManagementPage : ContentPage
{
    private bool instanced = false;

	public AdminPremiumManagementPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        this.Loaded += async (sender, e) =>
        {
            if (BindingContext is IViewModelInstancer vm && !instanced)
            {
                await Task.Yield();
                await vm.InitializeAsync();
                instanced = true;
            }
        };
    }

    private async void BackToMainPage(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}