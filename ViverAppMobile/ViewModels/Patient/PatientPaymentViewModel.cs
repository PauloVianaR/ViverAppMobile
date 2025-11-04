using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverApp.Shared.Utils;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Views.General;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Patient
{
    public partial class PatientPaymentViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly ScheduleService scheduleService;
        private readonly PaymentService paymentService;
        private readonly PagBankService pagBankService;
        private UserDto? user;
        private int paymentHistoricPage = 0;
        private const int pageSizeDefault = 10;

        [ObservableProperty] private int selectedScheduleId = 0;
        [ObservableProperty] private int selectedPayMethodFilter = 0;
        [ObservableProperty] private int selectedWherePaidFilter = 0;
        [ObservableProperty] private int paymentsHistoriCount = 0;
        [ObservableProperty] private int selectedPayMethod = (int)PayMethod.PIX;
        [ObservableProperty] private bool hasMoreHistoric = true;
        [ObservableProperty] private bool showingPaymentSide = true;
        [ObservableProperty] private bool isPayingOnline = true;
        [ObservableProperty] private bool selectedScheduleIsOnline = false;
        [ObservableProperty] private bool userHasPendingPayments = false;
        [ObservableProperty] private bool isShowingFilters = false;
        [ObservableProperty] private bool canPayOrConfirm = false;
        [ObservableProperty] private bool isRefresing = false;
        [ObservableProperty] private string payButtonText = "Selecione um atendimento";
        [ObservableProperty] private string minValueFilter = string.Empty;
        [ObservableProperty] private string maxValueFilter = string.Empty;
        [ObservableProperty] private DateTime selectedInitialDateFilter;
        [ObservableProperty] private DateTime selectedFinalDateFilter = DateTime.Today;

        public ObservableCollection<SelectableModel<ScheduleDto>> PendingAppointments { get; set; } = [];
        public ObservableCollection<PaymentHistoricDto> PaymentsHistoric { get; set; } = [];
        public ObservableCollection<string> PayMethodsPicker { get; } = ["Todos", "\ue805  Cartão", "\ue82b  PIX", "\uf0d6  Dinheiro", "\uf1a0  Google Pay"];
        public ObservableCollection<string> WherePaidFilter { get; } = ["Todos", "\ue82a  App", "\ue800  Presencial"];


        public PatientPaymentViewModel()
        {
            scheduleService = new();
            paymentService = new();
            pagBankService = new();
        }

        public async Task InitializeAsync()
        {
            SelectedInitialDateFilter = DateTimeHelper.GetSameDayLastYear();
            WeakReferenceMessenger.Default.Register<ShowPaymentPageSelectScheduleToPayByMainMessage>(this, (r, m) => SetSelectedScheduleId(m.Value));
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            try
            {
                SelectedInitialDateFilter = DateTimeHelper.GetSameDayLastYear();
                HasMoreHistoric = true;
                paymentHistoricPage = 0;

                user = UserHelper.GetLoggedUser();

                if (user is null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => UserHasPendingPayments = false);
                    return null;
                }

                StringBuilder sb = new();

                sb.AppendLine(await this.LoadMorePaymentHistoric());
                Master.GlobalToken.ThrowIfCancellationRequested();

                var scheduleResp = 
                    await scheduleService.GetScheduleAsync(user.IdUser, isDoctor:false, isHistoric:false, page:0, pagesize:int.MaxValue, (int)ScheduleStatus.Pending);
                if (!scheduleResp.WasSuccessful)
                    sb.AppendLine($"Erro ao carregar agenda:\n{scheduleResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                if (scheduleResp is null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => UserHasPendingPayments = false);
                    return null;
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PendingAppointments.Clear();
                    scheduleResp?.Response?.ToList().ForEach(s => PendingAppointments.Add(new SelectableModel<ScheduleDto>(s, s.IdSchedule, SelectedScheduleId)));

                    UserHasPendingPayments = PendingAppointments.Count > 0;
                });

                if (sb.Length > 0 && !string.IsNullOrEmpty(sb.ToString().Trim()))
                    throw new Exception(sb.ToString());

                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [RelayCommand]
        private async Task<string> LoadMorePaymentHistoric()
        {
            if (user is null || !HasMoreHistoric)
                return string.Empty;

            StringBuilder sb = new();

            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                decimal _minValue = decimal.TryParse(string.IsNullOrEmpty(MinValueFilter) ? "0" : MinValueFilter, out decimal minresult) ? minresult : 0m;
                decimal _maxValue = decimal.TryParse(string.IsNullOrEmpty(MaxValueFilter) ? "0" : maxValueFilter, out decimal maxresult) ? maxresult : 0m;

                var payResp = await paymentService.GetPaymentsByUser(user: user, page: paymentHistoricPage, pageSize: pageSizeDefault, initialDate: SelectedInitialDateFilter, finalDate: SelectedFinalDateFilter, minValue: _minValue, maxValue: _maxValue, paymentType: SelectedPayMethodFilter, wherePaid: SelectedWherePaidFilter);

                if (!payResp.WasSuccessful)
                    sb.AppendLine($"Erro ao carregar histórico de pagamentos:\n{payResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                var historic = payResp?.Response?.ToList();
                await MainThread.InvokeOnMainThreadAsync(() => historic.ForEach(p => PaymentsHistoric.Add(p)));
                if (historic.Count < pageSizeDefault)
                    HasMoreHistoric = false;
                else
                    paymentHistoricPage++;

                var payCountResp = await paymentService.GetPaymentsByUserCounting(user, SelectedInitialDateFilter, SelectedFinalDateFilter, _minValue, _maxValue, SelectedPayMethodFilter, SelectedWherePaidFilter);

                if (!payCountResp.WasSuccessful)
                    sb.AppendLine($"Erro ao carregar contagem de pagamentos:\n{payCountResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                await MainThread.InvokeOnMainThreadAsync(() => PaymentsHistoriCount = payCountResp.Response);

                if (sb.Length > 0 && !string.IsNullOrEmpty(sb.ToString().Trim()))
                    throw new Exception(sb.ToString());
            }
            catch (OperationCanceledException) { }
            catch (Exception)
            {
                return sb.ToString();
            }

            return string.Empty;
        }

        [RelayCommand] private void ShowPaymentSide() => ShowingPaymentSide = true;
        [RelayCommand] private void ShowHistoricSide() => ShowingPaymentSide = false;
        [RelayCommand] private void SwitchToProfile() => Navigator.SwitchPatientPage(PatientPage.Profile);
        [RelayCommand] private void ToogleShowingFilter() => IsShowingFilters = !IsShowingFilters;
        [RelayCommand] private void SwitchToSchedulePage() => WeakReferenceMessenger.Default.Send(new ShowSchedulePageSelectAppointmentMessage(0));

        [RelayCommand]
        private async Task FilterPaymentsHistoric()
        {
            PaymentsHistoric.Clear();
            paymentHistoricPage = 0;
            HasMoreHistoric = true;
            _ = await this.LoadMorePaymentHistoric();
        }

        [RelayCommand] private void SetSelectedScheduleId(int id)
        {
            SelectedScheduleId = id;

            foreach (var a in PendingAppointments)
            {
                a.SelectedId = SelectedScheduleId;
            }

            var schedule = PendingAppointments.FirstOrDefault(p => p.Model.IdSchedule == id);
            bool scheduleIsOnline = schedule?.Model?.IsOnline == (sbyte)1;
            SelectedScheduleIsOnline = scheduleIsOnline;

            if(scheduleIsOnline)
                IsPayingOnline = scheduleIsOnline;

            this.PayOrConfirmValidation();
        }

        [RelayCommand] private void SetSelectedPayMethod(int method)
        {
            SelectedPayMethod = method;
            this.PayOrConfirmValidation();
        }

        [RelayCommand]
        private void TooglePayingOnline(bool value)
        {
            if(IsPayingOnline != value)
            {
                SelectedPayMethod = 0;
            }

            IsPayingOnline = SelectedScheduleIsOnline || value;
            this.PayOrConfirmValidation();
        }

        private void PayOrConfirmValidation()
        {
            if (SelectedScheduleId != 0)
            {
                PayButtonText = IsPayingOnline ? "Pagar" : "Confirmar";
                CanPayOrConfirm = true;
                return;
            }

            CanPayOrConfirm = false;
        }

        [RelayCommand] private void PayOrConfirm()
        {
            if (!CanPayOrConfirm)
                return;

            if (PayButtonText == "Confirmar")
                this.ConfirmScheduleFlux();
            else
                this.PayScheduleOnlineFlux();
        }
        
        private async void ConfirmScheduleFlux()
        {
            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                await PopupHelper.PushLoadingAsync();
                var schedule = PendingAppointments.FirstOrDefault(s => s.SelectedId == s.Model.IdSchedule).Model;

                int? oldstatus = schedule.Status;
                schedule.Status = (int)ScheduleStatus.Confirmed;
                schedule.PendingPayment = 1;

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(schedule, (int)UserType.Patient, string.Empty, oldstatus));
                Master.GlobalToken.ThrowIfCancellationRequested();

                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(resp.ResponseErr);
                }

                await Messenger.ShowMessage("Agendamento confirmado, mas pendente de pagamento na clínica.", "Sucesso");
                WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(PatientPage.Payment.ToString()));
                await this.ReloadPage();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Messenger.ShowErrorMessage(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        private async void PayScheduleOnlineFlux()
        {
            try
            {
                var schedule = PendingAppointments.FirstOrDefault(s => s.SelectedId == s.Model.IdSchedule).Model
                    ?? throw new Exception("Selecione um atendimento para ser pago");

                await PopupHelper.PushLoadingAsync();

                var resp = await pagBankService.GetCheckoutLinkAsync(schedule);
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                var payUrl = resp.Response;
                if (string.IsNullOrWhiteSpace(payUrl))
                    throw new Exception("Falha ao tentar obter o link de pagamento.\nTente novamente");

                await PopupHelper.PopLoadingAsync();
                await Launcher.OpenAsync(payUrl);
            }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopLoadingAsync();
        }

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsRefresing)
                return;

            IsRefresing = true;
            await Task.Delay(1000);

            HasMoreHistoric = true;
            paymentHistoricPage = 0;
            SelectedScheduleId = 0;
            SelectedPayMethodFilter = 0;
            SelectedWherePaidFilter = 0;
            SelectedPayMethod = (int)PayMethod.PIX;
            PaymentsHistoriCount = 0;
            IsPayingOnline = true;
            SelectedScheduleIsOnline = false;
            UserHasPendingPayments = false;
            IsShowingFilters = false;
            CanPayOrConfirm = false;
            IsRefresing = false;
            PayButtonText = "Selecione uma consulta";
            MinValueFilter = string.Empty;
            MaxValueFilter = string.Empty;
            SelectedInitialDateFilter = DateTimeHelper.GetSameDayLastYear();
            SelectedFinalDateFilter = DateTime.Today;
            PendingAppointments.Clear();
            PaymentsHistoric.Clear();

            await Loader.RunWithLoadingAsync(LoadAllAsync);

            IsRefresing = false;
        }
    }
}
