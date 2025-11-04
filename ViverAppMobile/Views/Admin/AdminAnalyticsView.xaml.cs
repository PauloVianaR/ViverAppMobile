using ViverAppMobile.Controls;

namespace ViverAppMobile.Views.Admin;

public partial class AdminAnalyticsView : ContentView
{
    private bool instanced = false;

	public AdminAnalyticsView()
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
}