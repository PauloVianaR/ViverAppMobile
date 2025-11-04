using Mopups.Pages;
using ViverApp.Shared.DTos;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;

namespace ViverAppMobile.Views.Popups;

public partial class CancelSchedulePopup : PopupPage, IPopupAsync
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();
    public ScheduleDto? UserSchedule { get; set; }
    public bool IsPatient { get; set; } = false;

    public CancelSchedulePopup()
    {
        InitializeComponent();
        UserSchedule = PopupHelper<ScheduleDto>.GetValue();
        IsPatient = ValueBunker<bool>.SavedValue;
        BindingContext = this;

        CancelInfoLabel.Text = !IsPatient 
            ?"Ao cancelar, você pode marcar um novo atendimento a qualquer momento através do aplicativo ou ligando para a clínica."
            : "Ao cancelar, o paciente será avisado desta ação e poderá marcar outro agendamento se necessário.\nObs: Lembrando que esta ação de cancelamento não poderá ser desfeita.";
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

    private void YesButtonPressed(object sender, TappedEventArgs e)
    {
        string cancelReason = string.IsNullOrWhiteSpace(CancellationReason.Text) ? "NÃO INFORMADO" : CancellationReason.Text;

        PopupHelper<string>.SetValue(cancelReason);
        _taskCompletionSource.TrySetResult(true);
    }
}