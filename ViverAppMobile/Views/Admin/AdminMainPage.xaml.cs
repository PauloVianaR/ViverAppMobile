namespace ViverAppMobile.Views.Admin;

public partial class AdminMainPage : ContentPage
{
	public AdminMainPage()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        Title = string.Empty;

        this.Loaded += (sender, e) =>
        {
            NavigationPage.SetHasNavigationBar(this, false);
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NavigationPage.SetHasNavigationBar(this, false);
        Title = string.Empty;
    }
}