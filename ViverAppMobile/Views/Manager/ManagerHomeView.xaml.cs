using ViverAppMobile.Controls;

namespace ViverAppMobile.Views.Manager;

public partial class ManagerHomeView : ContentView
{
    private bool instanced = false;

	public ManagerHomeView()
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