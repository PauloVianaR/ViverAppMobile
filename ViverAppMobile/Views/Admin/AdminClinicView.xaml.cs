using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Models;
using ViverAppMobile.ViewModels.Admin;
namespace ViverAppMobile.Views.Admin;

public partial class AdminClinicView : ContentView
{
    private bool instanced = false;
    private readonly Dictionary<Expander, Label> iconLabelsInExpander = [];

	public AdminClinicView()
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

        iconLabelsInExpander.Add(AddNewAppointmentExpander, AddNewAppointmentIconLabelExpand);
        iconLabelsInExpander.Add(FilterExpander, FiltersIconLabelExpand);
        iconLabelsInExpander.Add(AddHolidayExpander, AddHolidayIconLabelExpand);
    }

    private void Expander_ExpandedChanged(object sender, ExpandedChangedEventArgs e)
    {
		var expander = ((Expander)sender);

        if (expander.Content is not VisualElement body)
            return;

        if (e.IsExpanded)
        {
            iconLabelsInExpander[expander].Text = "\ue83c";
            body.IsVisible = true;
            body.Opacity = 0;
            body.FadeTo(1, 600, Easing.CubicInOut);
        }
        else
        {
            iconLabelsInExpander[expander].Text = "\ue80e";
            body.FadeTo(0, 500, Easing.CubicInOut);
            body.IsVisible = false;
        }
    }

    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        if (sender is not View sw)
            return;

#if ANDROID
        if (!sw.IsFocused)
            return;
#endif

        if (sender is not Switch && sender is not CustomSwitch)
            return;

        if (this.BindingContext is not AdminClinicViewModel vm)
            return;

        if (sw.BindingContext is AsyncModel<Appointment> appnt)
        {
            appnt.Model.Status = e.Value ? 1 : 0;
            appnt.IsActive = e.Value;

            if (vm.ToggleAppointmentStatusCommand.CanExecute(appnt))
                vm.ToggleAppointmentStatusCommand.Execute(appnt);
        }

        if(sw.BindingContext is AsyncModel<AvailabilityClinic> ac)
        {
            ac.IsActive = e.Value;

            if (vm.ToggleAvailiabilityClinicStatusCommand.CanExecute(ac))
                vm.ToggleAvailiabilityClinicStatusCommand.Execute(ac);
        }

        if(sw.BindingContext is AsyncModel<Config> config)
        {
            config.Model.Value = e.Value ? 1 : 0;
            config.IsActive = e.Value;

            if(vm.ToggleConfigStatusCommand.CanExecute(config))
                vm.ToggleConfigStatusCommand.Execute(config);
        }
    }
}