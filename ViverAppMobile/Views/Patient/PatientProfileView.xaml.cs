using ViverAppMobile.Controls;
using ViverAppMobile.ViewModels.Patient;

namespace ViverAppMobile.Views.Patient;

public partial class PatientProfileView : ContentView
{
    private bool instanced = false;

    public PatientProfileView()
    {
        InitializeComponent();
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
