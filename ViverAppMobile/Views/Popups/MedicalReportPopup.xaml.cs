using Mopups.Pages;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Views.Popups;

public partial class MedicalReportPopup : PopupPage, IPopupAsync
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();
    public ScheduleDto? UserSchedule { get; set; }
    public bool CanEdit { get; set; }

    public MedicalReportPopup()
	{
        InitializeComponent();
        UserSchedule = PopupHelper<ScheduleDto>.GetValue();
        CanEdit = ValueBunker<bool?>.SavedValue ?? false;
        BindingContext = this;
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

    private void DownloadOrSaveMedicalReport(object sender, TappedEventArgs e)
    {
        Action reportAction = CanEdit ? SaveMedicalReport : DownloadMedicalReport;
        reportAction();
    }

    private async void DownloadMedicalReport()
    {
        await Messenger.ShowErrorMessageAsync("Funcionalidade indisponível no momento.");
    }

    private void SaveMedicalReport()
    {
        PopupHelper<string>.SetValue(UserSchedule.MedicalReport);
        _taskCompletionSource.TrySetResult(true);
    }
}