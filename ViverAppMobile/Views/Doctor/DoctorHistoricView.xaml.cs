using CommunityToolkit.Maui.Views;
using ViverAppMobile.Controls;

namespace ViverAppMobile.Views.Doctor;

public partial class DoctorHistoricView : ContentView
{
    private bool instanced = false;

    public DoctorHistoricView()
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

    private void FilterExpander_ExpandedChanged(object sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
    {
        var expander = ((Expander)sender);

        if (expander.Content is not VisualElement body)
            return;

        if (e.IsExpanded)
        {
            FiltersIconLabelExpand.Text = "\ue83c";
            body.IsVisible = true;
            body.Opacity = 0;
            body.FadeTo(1, 600, Easing.CubicInOut);
        }
        else
        {
            FiltersIconLabelExpand.Text = "\ue80e";
            body.FadeTo(0, 500, Easing.CubicInOut);
            body.IsVisible = false;
        }
    }
}