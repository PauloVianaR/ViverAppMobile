using ViverAppMobile.Controls;
using ViverAppMobile.ViewModels;

namespace ViverAppMobile.Views.Patient;

public partial class PatientScheduleView : ContentView
{
    private bool instanced = false;

	public PatientScheduleView()
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