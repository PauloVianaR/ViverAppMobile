using CommunityToolkit.Maui.Views;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Models;
using ViverAppMobile.ViewModels.Doctor;

namespace ViverAppMobile.Views.Doctor;

public partial class DoctorProfileView : ContentView
{
    private bool instanced = false;
    private Dictionary<Expander, Label> LabelsExpander = [];

    public DoctorProfileView()
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

        LabelsExpander.Add(ConsultationExpander, ConsultationIconLabelExpand);
        LabelsExpander.Add(ExaminationExpander, ExaminationIconLabelExpand);
        LabelsExpander.Add(SurgeryExpander, SurgeryIconLabelExpand);
        LabelsExpander.Add(AvDocOnlineExpander, AvDocOnlineIconLabelExpand);
        LabelsExpander.Add(AvDocPresencialExpander, AvDocPresencialIconLabelExpand);
    }

    private void Expander_ExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        var expander = ((Expander)sender);

        if (expander.Content is not VisualElement body)
            return;

        if (e.IsExpanded)
        {
            LabelsExpander[expander].Text = "\ue83c";
            body.IsVisible = true;
            body.Opacity = 0;
            body.FadeTo(1, 600, Easing.CubicInOut);
        }
        else
        {
            LabelsExpander[expander].Text = "\ue80e";
            body.FadeTo(0, 500, Easing.CubicInOut);
            body.IsVisible = false;
        }
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!((CheckBox)sender).IsFocused)
            return;

        if (sender is not View sw)
            return;

        if (this.BindingContext is not DoctorProfileViewModel vm)
            return;

        if (sw.BindingContext is AsyncModel<Appointment> appnt)
        {
            appnt.IsActive = e.Value;

            if (vm.SetSpeciltyDoctorCommand.CanExecute(appnt))
                vm.SetSpeciltyDoctorCommand.Execute(appnt);
        }
    }

    private void CustomSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (sender is not View)
            return;

        if (sender is not CustomSwitch)
            return;

        if (this.BindingContext is not DoctorProfileViewModel vm)
            return;

        sbyte attendonline = e.Value ? (sbyte)1 : (sbyte)0;

        if (vm.ToggleAttendOnlineCommand.CanExecute(attendonline))
            vm.ToggleAttendOnlineCommand.Execute(attendonline);
    }

    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        if (sender is not View sw)
            return;

        if (!sw.IsFocused)
            return;

        if (sender is not Switch)
            return;

        if (this.BindingContext is not DoctorProfileViewModel vm)
            return;

        if (sw.BindingContext is not AsyncModel<AvailabilityDoctor> ad)
            return;

        ad.IsActive = e.Value;

        if (vm.ToggleAvailiabilityClinicStatusCommand.CanExecute(ad))
            vm.ToggleAvailiabilityClinicStatusCommand.Execute(ad);
    }
}