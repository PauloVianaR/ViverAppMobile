using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Patient
{
    public partial class PatientAgendaViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly ScheduleService scheduleService;
        private readonly ScheduleAttachmentsService scheduleAttachmentsService;
        private UserDto? user;
        private const int pageSizeDefault = 4;
        private int pageSchedule = 0;
        private int pageHistoric = 0;
        private int statusFilter = 0;
        private bool isLoading = false;

        [ObservableProperty] private int nextAppointmentsCount = 0;
        [ObservableProperty] private int historicAppointmentsCount = 0;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private bool isShowingHistoric = false;
        [ObservableProperty] private bool isLoadingHistoric = true;
        [ObservableProperty] private bool hasMoreSchedule = true;
        [ObservableProperty] private bool hasMoreHistoric = true;
        [ObservableProperty] private string selectedStatusFilter;
        [ObservableProperty] private string filterString = string.Empty;

        public ObservableCollection<string> StatusNames { get; }
        public ObservableCollection<ScheduleDto> Schedules { get; set; } = [];
        public ObservableCollection<ScheduleDto> Historic { get; set; } = [];

        public PatientAgendaViewModel()
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

                pageSchedule = 0;
                pageHistoric = 0;

                var loggedUser = UserHelper.GetLoggedUser();
                if (loggedUser is null)
                    return null;

                user = loggedUser;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Schedules.Clear();
                    Historic.Clear();
                    HasMoreSchedule = true;
                    HasMoreHistoric = true;
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
            finally
            {
                isLoading = false;
            }
        }

        [RelayCommand]
        private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;

            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }


        private async Task<string> LoadScheduleCounter()
        {
            if (user is null)
                return string.Empty;

            try
            {
                var respScheduleCounter = await scheduleService.GetScheduleCountAsync(user.IdUser, isDoctor:false, countingHistoric:false, statusFilter, FilterString);
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
            if (user is null)
                return string.Empty;

            try
            {
                var respHistoricCounter = await scheduleService.GetScheduleCountAsync(user.IdUser, isDoctor: false, countingHistoric: true, statusFilter, FilterString);
                if (!respHistoricCounter.WasSuccessful)
                    throw new Exception(respHistoricCounter.ResponseErr);

                await MainThread.InvokeOnMainThreadAsync(() => HistoricAppointmentsCount = respHistoricCounter?.Response ?? 0);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return string.Empty;
        }

        [RelayCommand]
        private async Task<string> LoadMoreHistoric()
        {
            try
            {
                if (user is null || !HasMoreHistoric)
                    return string.Empty;

                var resp = await scheduleService.GetScheduleAsync(user.IdUser, isDoctor: false, isHistoric: true, page: pageHistoric, pagesize: pageSizeDefault, statusFilter, FilterString);

                if (!resp.WasSuccessful)
                    throw new Exception(resp.ResponseErr);

                var historic = resp?.Response?.ToList() ?? [];
                await MainThread.InvokeOnMainThreadAsync(() => historic.ForEach(s => Historic.Add(s)));

                if (historic.Count < pageSizeDefault)
                    HasMoreHistoric = false;
                else
                    pageHistoric++;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return string.Empty;
        }

        [RelayCommand]
        private async Task<string> LoadMoreSchedule()
        {
            try
            {
                if (user is null || !HasMoreSchedule)
                    return string.Empty;
               
                var resp = await scheduleService.GetScheduleAsync(user.IdUser, isDoctor: false, isHistoric: false, page: pageSchedule, pagesize: pageSizeDefault, statusFilter, FilterString);

                if (!resp.WasSuccessful)
                    throw new Exception(resp.ResponseErr);

                var schedules = resp?.Response?.ToList() ?? [];
                await MainThread.InvokeOnMainThreadAsync(() => schedules.ForEach(s => Schedules.Add(s)));

                if (schedules.Count < pageSizeDefault)
                    HasMoreSchedule = false;
                else
                    pageSchedule++;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return string.Empty;
        }

        [RelayCommand] private async Task ToggleShowHistoric(bool value)
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
            statusFilter = StatusNames.IndexOf(SelectedStatusFilter);
            statusFilter = statusFilter == -1 ? 0 : statusFilter;
            _ = Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        [RelayCommand] private async Task SearchScheduleByFilter()
        {
            FilterString = FilterString.ToLower().Trim();
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        [RelayCommand] private void SelectAppointmentToPay(ScheduleDto s) => WeakReferenceMessenger.Default.Send(new ShowPaymentPageSelectScheduleToPayMessage(s.IdSchedule));

        [RelayCommand] private async Task CancelSchedule(ScheduleDto dto)
        {
            ValueBunker<bool>.SavedValue = false;
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

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(dto, (int)UserType.Patient, string.Empty, oldstatus));
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

        [RelayCommand] private async Task Reschedule(ScheduleDto dto)
        {
            var returnValue = await PopupHelper<ScheduleDto>.PushInstanceAsync<ReschedulePopup>(dto);

            if (!returnValue)
                return;

            await Messenger.ShowMessage("Consulta reagendada\n\nFique atento ao calendário emm 😉!", "Sucesso");
            await this.ReloadPage();
        }

        [RelayCommand] private async Task HowToGetThereOrSwitchOnlinePage(ScheduleDto dto)
        {
            if (dto.IsOnline == 1)
                await SwitchOnlinePage(dto);
            else
                HowToGetThere(dto);
        }

        private async void HowToGetThere(ScheduleDto dto) => await MapsHelper.OpenRouteByCepAsync(cep: dto.ClinicPostalCode, number: dto.ClinicNumber);
        private async Task SwitchOnlinePage(ScheduleDto schedule) => await Navigator.PushOnlinePage(schedule);
        [RelayCommand] private async Task ShowMedicalReport(ScheduleDto dto) => _ = await PopupHelper<ScheduleDto>.PushInstanceAsync<MedicalReportPopup>(dto);
        [RelayCommand] private async Task CallClinic(ScheduleDto dto) => await PhoneHelper.CallAsync(dto.ClinicPhone);

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

                ValueBunker<int, bool>.Build(dto.IdSchedule, false);
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

        [RelayCommand] private async Task RateAppointment(ScheduleDto dto)
        {
            var returnValue = await PopupHelper<ScheduleDto>.PushInstanceAsync<RateSchedulePopup>(dto);

            if (!returnValue)
                return;

            try
            {
                var schedule = PopupHelper<ScheduleDto>.GetValue();
                if (schedule is null)
                    return;

                int? oldstatus = dto.Status;
                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(schedule, (int)UserType.Patient,string.Empty, oldstatus));
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(resp.ResponseErr);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }
    }
}
