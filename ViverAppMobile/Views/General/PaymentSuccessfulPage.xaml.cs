using ViverAppMobile.Workers;

namespace ViverAppMobile.Views.General;

public partial class PaymentSuccessfulPage : ContentPage
{
	public PaymentSuccessfulPage()
	{
		InitializeComponent();
	}

    private void BackToLabel(object sender, TappedEventArgs e)
    {
		_ = Navigator.PopNavigationAsync();
    }

    private void BackTo(object sender, EventArgs e)
    {
        _ = Navigator.PopNavigationAsync();
    }
}