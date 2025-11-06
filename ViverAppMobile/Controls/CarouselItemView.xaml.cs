namespace ViverAppMobile.Controls;

public partial class CarouselItemView : ContentView
{
	public CarouselItemView()
	{
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("=== CarouselItemView InitializeComponent EXCEPTION ===");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            throw;
        }
    }
}