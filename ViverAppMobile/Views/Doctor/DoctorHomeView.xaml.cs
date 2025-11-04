using ViverAppMobile.Controls;

namespace ViverAppMobile.Views.Doctor;

public partial class DoctorHomeView : ContentView
{
    private bool instanced = false;

    public DoctorHomeView()
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