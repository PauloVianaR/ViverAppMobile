using Mopups.Pages;
using ViverAppMobile.Controls;
using ViverAppMobile.ViewModels.Popups;

namespace ViverAppMobile.Views.Popups;

public partial class ReschedulePopup : PopupPage, IPopupAsync
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();

    public ReschedulePopup()
	{
        InitializeComponent();
        this.Loaded += async (sender, e) =>
        {
            if (BindingContext is IViewModelInstancer vm)
            {
                await Task.Yield();
                await vm.InitializeAsync();
            }
        };
    }

    public Task<object?> WaitForResultAsync() => _taskCompletionSource.Task;
    protected override bool OnBackButtonPressed() => ClosePopup();
    protected override bool OnBackgroundClicked() => ClosePopup();
    private void BackButtonPressed(object sender, TappedEventArgs e) => ClosePopup();

    public bool ClosePopup()
    {
        _taskCompletionSource.TrySetResult(false);
        return base.OnBackgroundClicked();
    }

    private void YesButtonPressed(object sender, TappedEventArgs e) => _ = YesButtonPressedAsync(sender, e);

    private async Task YesButtonPressedAsync(object sender, TappedEventArgs e)
    {
        if (BindingContext is not ReschudlePopupViewModel vm)
            return;

        if(vm.RescheduleCommand.CanExecute(null))
            await vm.RescheduleCommand.ExecuteAsync(null);

        _taskCompletionSource.TrySetResult(true);
    }
}