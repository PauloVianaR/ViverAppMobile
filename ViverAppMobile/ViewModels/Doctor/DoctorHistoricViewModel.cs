using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverApp.Shared.Utils;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Doctor
{
    public partial class DoctorHistoricViewModel : ObservableObject, IViewModelInstancer
    {
        private int page = 0;
        private const int pageSizeDefault = 10;
        private bool isLoading = false;
        private bool isBusy = false;
        private readonly ScheduleService scheduleService;
        private readonly ScheduleAttachmentsService scheduleAttachmentsService;
        private UserDto? user;

        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private bool hasMoreHistoric = true;
        [ObservableProperty] private string filterString = string.Empty;
        [ObservableProperty] private string selectedModalityFilter = string.Empty;
        [ObservableProperty] private string selectedAppointmentTypeFilter = string.Empty;
        [ObservableProperty] private DateTime startDateFilter = DateTimeHelper.GetFirstDayThisMonth();
        [ObservableProperty] private DateTime endDateFilter = DateTimeHelper.GetLastDayThisMonth();
        [ObservableProperty] private ObservableCollection<ScheduleDto> historic = [];

        public List<string> FilterModalityOptions { get; } = ["Todas", "Presencial", "Online"];
        public List<string> FilterAppointmentTypes { get; } = ["Todos", "Consulta", "Exame", "Cirurgia"];

        public DoctorHistoricViewModel()
        {
            scheduleService = new();
            scheduleAttachmentsService = new();

            SelectedModalityFilter = FilterModalityOptions[0];
            SelectedAppointmentTypeFilter = FilterAppointmentTypes[0];
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

            user = UserHelper.GetLoggedUser();
            PopupHelper.CanActivateSecondLoadingPopup = false;
            await LoadMoreHistoric();

            PopupHelper.CanActivateSecondLoadingPopup = true;
            isLoading = false;
            return null;            
        }

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        [RelayCommand] private void ClearFilters()
        {
            FilterString = string.Empty;
            SelectedModalityFilter = FilterModalityOptions[0];
            SelectedAppointmentTypeFilter = FilterAppointmentTypes[0];
            StartDateFilter = DateTimeHelper.GetFirstDayThisMonth();
            EndDateFilter = DateTimeHelper.GetLastDayThisMonth();
        }

        [RelayCommand] private async Task LoadMoreHistoric()
        {
            if (user is null)
                return;

            string? errs = await Task.Run(async Task<string?>?() =>
            {
                try
                {
                    if (!HasMoreHistoric)
                        return null;

                    int modalityIndex = FilterModalityOptions.IndexOf(SelectedModalityFilter);
                    if (modalityIndex < 0)
                        modalityIndex = 0;

                    int appntType = FilterAppointmentTypes.IndexOf(SelectedAppointmentTypeFilter);
                    if(appntType < 0)
                        appntType = 0;

                    var resp = await scheduleService.GetScheduleAsync(id: user.IdUser, isDoctor: true, isHistoric: true, page: page, pagesize: pageSizeDefault, filterStatus: (int)ScheduleStatus.Concluded, filterString: FilterString, initialdate: startDateFilter, finaldate: endDateFilter, modalityfilter: modalityIndex, appointmenttypefilter: appntType);

                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }

                    var historic = resp?.Response?.ToList() ?? [];
                    await MainThread.InvokeOnMainThreadAsync(() => historic.ForEach(h => Historic.Add(h)));

                    if (historic.Count < pageSizeDefault)
                        HasMoreHistoric = false;
                    else
                        page++;

                    return null;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }
            });

            if (!string.IsNullOrWhiteSpace(errs))
                await MainThread.InvokeOnMainThreadAsync(async () => await Messenger.ShowErrorMessageAsync(errs));

            if(PopupHelper.CanActivateSecondLoadingPopup)
                await MainThread.InvokeOnMainThreadAsync(async () => await PopupHelper.PopAllPopUpAsync());
        }

        private async void FilterCollection()
        {
            if (isBusy)
                return;
            isBusy = true;

            Historic.Clear();
            page = 0;
            HasMoreHistoric = true;
            await LoadMoreHistoric();

            isBusy = false;
        }

        [RelayCommand] private void FilterByString() => this.FilterCollection();
        partial void OnSelectedAppointmentTypeFilterChanged(string value) => this.FilterCollection();
        partial void OnSelectedModalityFilterChanged(string value) => this.FilterCollection();
        partial void OnStartDateFilterChanged(DateTime value) => this.FilterCollection();
        partial void OnEndDateFilterChanged(DateTime value) => this.FilterCollection();

        [RelayCommand] private async Task ShowAttachments(ScheduleDto dto)
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

        [RelayCommand] private async Task ShowMedicalReport(ScheduleDto schedule)
        {
            ValueBunker<bool?>.SavedValue = true;
            var result = await PopupHelper<ScheduleDto>.PushInstanceAsync<MedicalReportPopup>(schedule);
            if (!result)
                return;

            try
            {
                await PopupHelper.PushLoadingAsync();

                string modifiedReport = PopupHelper<string>.GetValue() ?? string.Empty;
                var existingSchedule = Historic.FirstOrDefault(s => s.IdSchedule == schedule.IdSchedule);
                if (existingSchedule is null)
                    return;

                int? oldstatus = existingSchedule.Status;
                existingSchedule.MedicalReport = modifiedReport;
                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(existingSchedule, (int)UserType.Doctor, user.Name ?? "Médico", oldstatus));
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                int historicIndex = Historic.IndexOf(existingSchedule);
                if (historicIndex >= 0)
                {
                    Historic[historicIndex] = existingSchedule;
                }

                await Messenger.ShowToastMessage("Laudo Alterado");
            }
            catch (OperationCanceledException) { }
            catch(Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            ValueBunker<bool?>.SavedValue = false;
            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand] public async Task ShowScheduleFeedback(ScheduleDto schedule)
        {
            string feedback = string.IsNullOrWhiteSpace(schedule.FeedBack) ? "[SEM FEEDBACK]" : schedule.FeedBack;

            await Messenger.ShowMessage(feedback, "Feedback do Paciente");
        }
    }
}
