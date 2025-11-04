using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;

namespace ViverAppMobile.Views.General;

public partial class OnlinePage : ContentPage
{
    private bool instanced = false;

	public OnlinePage()
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