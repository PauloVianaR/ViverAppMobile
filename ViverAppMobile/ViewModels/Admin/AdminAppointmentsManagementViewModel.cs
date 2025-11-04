using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverApp.Shared.Utils;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Admin
{
    public partial class AdminAppointmentsManagementViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly ScheduleService scheduleService;
        private readonly ScheduleAttachmentsService scheduleAttachmentsService;
        private const int pageSizeDefault = 6;
        private int _pageSchedule = 0;
        private int _pageHistoric = 0;
        private int statusFilter = 0;
        private bool isLoading = false;

        [ObservableProperty] private int nextAppointmentsCount = 0;
        [ObservableProperty] private int historicAppointmentsCount = 0;
        [ObservableProperty] private int remainingItemsSchedule = -1;
        [ObservableProperty] private int remainingItemsHistoric = -1;
        [ObservableProperty] private bool isRefreshing = false;
        [ObservableProperty] private bool isShowingHistoric = false;
        [ObservableProperty] private bool isLoadingHistoric = true;
        [ObservableProperty] private bool hasMoreSchedule = true;
        [ObservableProperty] private bool hasMoreHistoric = true;
        [ObservableProperty] private string selectedStatusFilter;
        [ObservableProperty] private string filterString = string.Empty;
        [ObservableProperty] private DateTime selectedInitialDateFilter = DateTimeHelper.GetSameDayLastYear();
        [ObservableProperty] private DateTime selectedFinalDateFilter = DateTime.Today.AddMonths(3);

        public ObservableCollection<string> StatusNames { get; }
        public ObservableCollection<ScheduleDto> Schedules { get; set; } = [];
        public ObservableCollection<ScheduleDto> Historic { get; set; } = [];

        public AdminAppointmentsManagementViewModel()
        {
            scheduleService = new();
            scheduleAttachmentsService = new();

            StatusNames = ["Todos os status", "Pendentes", "Confirmadas", "Realizadas", "Canceladas", "Reagendadas"];
            SelectedStatusFilter = StatusNames[0];
        }

        public async Task InitializeAsync()
        {
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            if (isLoading)
                return null;
            isLoading = true;

            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    HasMoreSchedule = true;
                    HasMoreHistoric = true;
                    NextAppointmentsCount = 0;
                    HistoricAppointmentsCount = 0;
                    Schedules.Clear();
                    Historic.Clear();
                });

                var err = await this.LoadScheduleCounter();
                if (!string.IsNullOrWhiteSpace(err))
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(err);
                }

                err = await this.LoadHistoricCounter();
                if (!string.IsNullOrWhiteSpace(err))
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(err);
                }

                err = await this.LoadMoreSchedule();
                if (!string.IsNullOrWhiteSpace(err))
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(err);
                }

                err = await this.LoadMoreHistoric();
                if (!string.IsNullOrWhiteSpace(err))
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(err);
                }

                await Task.Delay(500);

                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Master.CancelGlobalToken();
                return ex.Message;
            }
            finally
            {
                isLoading = false;
                RemainingItemsHistoric = 2;
                RemainingItemsSchedule = 2;
            }
        }

        [RelayCommand]
        private async Task ReloadPage()
        {
            if (IsRefreshing)
                return;
            IsRefreshing = true;

            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsRefreshing = false;
        }

        private async Task<string> LoadScheduleCounter()
        {
            try
            {
                var respScheduleCounter = await scheduleService.GetScheduleCountAsync(id:0, isDoctor:false, countingHistoric:false, statusFilter, FilterString, SelectedInitialDateFilter, SelectedFinalDateFilter);
                if (!respScheduleCounter.WasSuccessful)
                    throw new Exception(respScheduleCounter.ResponseErr);

                await MainThread.InvokeOnMainThreadAsync(() => NextAppointmentsCount = respScheduleCounter?.Response ?? 0);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return string.Empty;
        }

        private async Task<string> LoadHistoricCounter()
        {
            try
            {
                var respHistoricCounter = await scheduleService.GetScheduleCountAsync(id: 0, isDoctor: false, countingHistoric: true, statusFilter, FilterString, SelectedInitialDateFilter, SelectedFinalDateFilter);
                if (!respHistoricCounter.WasSuccessful)
                    throw new Exception(respHistoricCounter.ResponseErr);

                await MainThread.InvokeOnMainThreadAsync(() => HistoricAppointmentsCount = respHistoricCounter?.Response ?? 0);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return string.Empty;
        }

        [RelayCommand]
        private async Task<string?> LoadMoreHistoric()
        {
            try
            {
                if (!HasMoreHistoric)
                    return string.Empty;

                if (!isLoading && !IsShowingHistoric)
                    return string.Empty;

                var resp = await scheduleService.GetScheduleAsync(0, isDoctor: default, isHistoric: true, page: _pageHistoric, pagesize: pageSizeDefault, statusFilter, FilterString, SelectedInitialDateFilter, SelectedFinalDateFilter);

                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                var historic = resp?.Response?.ToList() ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    historic.ForEach(s => Historic.Add(s));
                    _pageHistoric++;
                });

                if (historic.Count < pageSizeDefault)
                {
                    HasMoreHistoric = false;
                    RemainingItemsHistoric = -1;
                }
            }
            catch (OperationCanceledException)
            {
                _pageHistoric = 0;
                hasMoreHistoric = false;
                RemainingItemsHistoric = -1;
            }
            catch (Exception ex)
            {
                _pageHistoric = 0;
                hasMoreHistoric = false;
                RemainingItemsHistoric = -1;
                return ex.Message;
            }

            return string.Empty;
        }

        [RelayCommand]
        private async Task<string?> LoadMoreSchedule()
        {
            try
            {
                if (!HasMoreSchedule)
                    return string.Empty;

                if (!isLoading && IsShowingHistoric)
                    return string.Empty;

                var resp = await scheduleService.GetScheduleAsync(0, isDoctor: default, isHistoric: false, page: _pageSchedule, pagesize: pageSizeDefault, statusFilter, FilterString, SelectedInitialDateFilter, SelectedFinalDateFilter);

                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                var schedules = resp?.Response?.ToList() ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    schedules.ForEach(s => Schedules.Add(s));
                    _pageSchedule++;
                });

                if (schedules.Count < pageSizeDefault)
                {
                    HasMoreSchedule = false;
                    RemainingItemsSchedule = -1;
                }
            }
            catch (OperationCanceledException)
            {
                _pageSchedule = 0;
                HasMoreSchedule = false;
                RemainingItemsSchedule = -1;
            }
            catch (Exception ex)
            {
                _pageSchedule = 0;
                HasMoreSchedule = false;
                RemainingItemsSchedule = -1;
                return ex.Message;
            }

            return string.Empty;
        }

        [RelayCommand]
        private async Task ToogleShowHistoric(bool value)
        { 
            IsShowingHistoric = value;
            if (IsLoadingHistoric)
            {
                await Task.Delay(Loader.LoadTime);
                IsLoadingHistoric = false;
            }
        }

        partial void OnSelectedStatusFilterChanged(string value)
        {
            if (isLoading)
                return;
            statusFilter = StatusNames.IndexOf(SelectedStatusFilter);
            statusFilter = statusFilter == -1 ? 0 : statusFilter;
            this.ResetPages();
            _ = Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        partial void OnSelectedInitialDateFilterChanged(DateTime value)
        {
            if (isLoading)
                return;
            this.ResetPages();
            _ = Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        partial void OnSelectedFinalDateFilterChanged(DateTime value)
        {
            if (isLoading)
                return;
            this.ResetPages();
            _ = Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        private void ResetPages()
        {
            _pageSchedule = 0;
            _pageHistoric = 0;
        }

        [RelayCommand]
        private async Task SearchScheduleByFilter()
        {
            FilterString = FilterString.ToLower().Trim();
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        [RelayCommand] private void SelectAppointmentToPay(ScheduleDto s) => WeakReferenceMessenger.Default.Send(new ShowPaymentPageSelectScheduleToPayMessage(s.IdSchedule));

        [RelayCommand]
        private async Task CancelSchedule(ScheduleDto dto)
        {
            ValueBunker<bool>.SavedValue = true;
            var returnValue = await PopupHelper<ScheduleDto>.PushInstanceAsync<CancelSchedulePopup>(dto);
            if (!returnValue)
                return;

            try
            {
                await PopupHelper.PushLoadingAsync();

                int? oldstatus = dto.Status;
                dto.Status = (int)ScheduleStatus.Canceled;
                dto.PendingPayment = 0;
                dto.MedicalReport = null;
                dto.Rating = null;
                dto.FeedBack = PopupHelper<string>.GetValue();

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(dto, (int)UserType.Admin, string.Empty,oldstatus));
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(resp.ResponseErr);
                }
                await this.ReloadPage();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand]
        private async Task Reschedule(ScheduleDto dto)
        {
            var returnValue = await PopupHelper<ScheduleDto>.PushInstanceAsync<ReschedulePopup>(dto);

            if (!returnValue)
                return;

            await Messenger.ShowMessage("Consulta reagendada\n\nFoi enviada uma notificação para o paciente!", "Sucesso");
            await this.ReloadPage();
        }

        [RelayCommand]
        private async Task ShowMedicalReport(ScheduleDto dto)
        {
            _ = await PopupHelper<ScheduleDto>.PushInstanceAsync<MedicalReportPopup>(dto);
        }

        [RelayCommand]
        private async Task ShowAttachments(ScheduleDto dto)
        {
            try
            {
                await PopupHelper.PushLoadingAsync();

                var resp = await scheduleAttachmentsService.GetAllByIdSchedule(dto.IdSchedule);
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                var attachments = resp.Response ?? [];
                await PopupHelper.PopLoadingAsync();

                ValueBunker<int, bool>.Build(dto.IdSchedule, true);
                await PopupHelper<IEnumerable<ScheduleAttachment>>.PushInstanceAsync<AppointmentAttachmentsPopup>(attachments);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }
            finally
            {
                await PopupHelper.PopLoadingAsync();
            }
        }
    }
}
