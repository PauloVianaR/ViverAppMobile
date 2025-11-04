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


namespace ViverAppMobile.ViewModels.Manager
{
    public partial class ManagerHistoricViewModel : ObservableObject, IViewModelInstancer
    {
        private const int pageSizeDefault = 10;
        private int page = 0;
        private bool isLoading = false;
        private bool isBusy = false;
        private readonly ScheduleService scheduleService;
        private readonly ScheduleAttachmentsService scheduleAttachmentsService;
        private readonly UserService userService;
        private UserDto? user;
        private List<DoctorDto> allDoctors = [];

        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private bool hasMoreHistoric = false;
        [ObservableProperty] private string filterString = string.Empty;
        [ObservableProperty] private string selectedModalityFilter = string.Empty;
        [ObservableProperty] private string selectedAppointmentTypeFilter = string.Empty;
        [ObservableProperty] private string selectedDoctorFilter = string.Empty;
        [ObservableProperty] private DateTime startDateFilter = DateTimeHelper.GetFirstDayThisMonth();
        [ObservableProperty] private DateTime endDateFilter = DateTimeHelper.GetLastDayThisMonth();
        [ObservableProperty] private ObservableCollection<ScheduleDto> historic = [];

        public List<string> FilterModalityOptions { get; } = ["Todas", "Presencial", "Online"];
        public List<string> FilterAppointmentTypes { get; } = ["Todos", "Consulta", "Exame", "Cirurgia"];
        public ObservableCollection<string> ActiveDoctors { get; set; } = ["Todos"];

        public ManagerHistoricViewModel()
        {
            scheduleService = new();
            scheduleAttachmentsService = new();
            userService = new();

            SelectedModalityFilter = FilterModalityOptions[0];
            SelectedAppointmentTypeFilter = FilterAppointmentTypes[0];
            SelectedDoctorFilter = ActiveDoctors[0];
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

            try
            {
                var resp = await userService.GetDoctorsAsync();
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                var doctors = resp?.Response ?? [];

                allDoctors = doctors.ToList();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    doctors.OrderBy(d => d.ProfessionalDoctorName)
                    .ToList()
                    .ForEach(d => ActiveDoctors.Add(d.ProfessionalDoctorName));
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                return ex.Message;
            }
            isLoading = false;
            return null;
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

        [RelayCommand]
        private void ClearFilters()
        {
            FilterString = string.Empty;
            SelectedModalityFilter = FilterModalityOptions[0];
            SelectedAppointmentTypeFilter = FilterAppointmentTypes[0];
            StartDateFilter = DateTimeHelper.GetFirstDayThisMonth();
            EndDateFilter = DateTimeHelper.GetLastDayThisMonth();
            SelectedDoctorFilter = ActiveDoctors[0];
        }

        [RelayCommand]
        private async Task LoadMoreHistoric()
        {
            if (user is null || !HasMoreHistoric)
                return;

            string? errs = await Task.Run(async Task<string?>? () =>
            {
                try
                {
                    int modalityIndex = FilterModalityOptions.IndexOf(SelectedModalityFilter);
                    if (modalityIndex < 0)
                        modalityIndex = 0;

                    int appntType = FilterAppointmentTypes.IndexOf(SelectedAppointmentTypeFilter);
                    if (appntType < 0)
                        appntType = 0;

                    bool isDoctorFiltring = false;
                    int idUserFilter = 0;

                    int selectedDoctorIndex = ActiveDoctors.IndexOf(SelectedDoctorFilter);
                    if (selectedDoctorIndex > 0)
                    {
                        var doctor = allDoctors.FirstOrDefault(d => d.ProfessionalDoctorName == SelectedDoctorFilter);
                        if (doctor is not null)
                        {
                            idUserFilter = doctor.IdUser;
                            isDoctorFiltring = true;
                        }
                    }

                    var resp = await scheduleService.GetScheduleAsync(id: idUserFilter, isDoctor: isDoctorFiltring, isHistoric: true, page: page, pagesize: pageSizeDefault, filterStatus: (int)ScheduleStatus.Concluded, filterString: FilterString, initialdate: startDateFilter, finaldate: endDateFilter, modalityfilter: modalityIndex, appointmenttypefilter: appntType);

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
                catch (Exception ex)
                {
                    return ex.Message;
                }
            });

            if (!string.IsNullOrWhiteSpace(errs))
                await MainThread.InvokeOnMainThreadAsync(async () => await Messenger.ShowErrorMessageAsync(errs));

            if (PopupHelper.CanActivateSecondLoadingPopup)
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
        partial void OnSelectedDoctorFilterChanged(string value) => this.FilterCollection();

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

        [RelayCommand]
        private async Task ShowMedicalReport(ScheduleDto schedule)
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

                int? status = existingSchedule.Status;
                existingSchedule.MedicalReport = modifiedReport;
                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(existingSchedule, (int)UserType.Manager, user.Name ?? "GERENTE", status));
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
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            ValueBunker<bool?>.SavedValue = false;
            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand]
        public async Task ShowScheduleFeedback(ScheduleDto schedule)
        {
            string feedback = string.IsNullOrWhiteSpace(schedule.FeedBack) ? "[SEM FEEDBACK]" : schedule.FeedBack;

            await Messenger.ShowMessage(feedback, "Feedback do Paciente");
        }
    }
}
