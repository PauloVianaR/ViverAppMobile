using Mopups.Pages;
using ViverApp.Shared.DTos;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;

namespace ViverAppMobile.Views.Popups;

public partial class ScheduleDetailsPopup : PopupPage, IPopupAsync
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();
    public ScheduleDto? UserSchedule { get; set; }

    public ScheduleDetailsPopup()
	{
        InitializeComponent();
        UserSchedule = PopupHelper<ScheduleDto>.GetValue();
        BindingContext = this;
    }

    public bool ClosePopup()
    {
        _taskCompletionSource.TrySetResult(false);
        return base.OnBackgroundClicked();
    }

    public Task<object?> WaitForResultAsync() => _taskCompletionSource.Task;
    protected override bool OnBackButtonPressed() => ClosePopup();
    protected override bool OnBackgroundClicked() => ClosePopup();
    private void BackButtonPressed(object sender, TappedEventArgs e) => ClosePopup();
}