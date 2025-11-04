namespace ViverAppMobile.Views.Manager;

public partial class ManagerMainPage : ContentPage
{
	public ManagerMainPage()
	{
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        Title = string.Empty;
    }
}