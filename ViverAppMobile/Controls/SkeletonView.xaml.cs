namespace ViverAppMobile.Controls;

public partial class SkeletonView : ContentView
{
	public SkeletonView()
	{
		InitializeComponent();

		this.Loaded += async (s, e) =>
		{
            await Task.Yield();
			Skeleton.IsVisible = true;
		};
	}
}