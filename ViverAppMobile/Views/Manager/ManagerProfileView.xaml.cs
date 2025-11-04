using ViverAppMobile.Controls;

namespace ViverAppMobile.Views.Manager;

public partial class ManagerProfileView : ContentView
{
	private bool instanced = false;

	public ManagerProfileView()
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