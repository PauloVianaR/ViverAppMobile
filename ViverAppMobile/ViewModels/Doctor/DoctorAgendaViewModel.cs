using CommunityToolkit.Maui.Core.Extensions;
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
using ViverAppMobile.Views.General;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Doctor
{
    public partial class DoctorAgendaViewModel : ObservableObject, IViewModelInstancer
    {
        private bool isLoading = false;
        private bool isFiltring = false;
        private readonly ScheduleService scheduleService;
        private List<ScheduleDto> allSchedule = [];

        [ObservableProperty] private int onlineTot = 0;
        [ObservableProperty] private int presencialTot = 0;
        [ObservableProperty] private int rescheduleTot = 0;
        [ObservableProperty] private int generalTot = 0;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private string filterTimePreposition = "de";
        [ObservableProperty] private string selectedFilterTime = string.Empty;
        [ObservableProperty] private string selectedFilterTimeTermination = "Hoje";
        [ObservableProperty] private string selectedFilterTimeTerminationReschedule = "para Hoje";
        [ObservableProperty] private string filterString = string.Empty;
        [ObservableProperty] private string selectedModalityFilter = string.Empty;
        [ObservableProperty] private string selectedAppointmentTypeFilter = string.Empty;
        [ObservableProperty] private DateTime startDateFilter = DateTime.Today;
        [ObservableProperty] private DateTime endDateFilter = DateTime.Today;
        [ObservableProperty] private TimeOnly startTimeFilter = TimeOnly.MinValue;
        [ObservableProperty] private TimeOnly endTimeFilter = TimeOnly.MaxValue;
        [ObservableProperty] private ObservableCollection<ScheduleDto> doctorSchedule = [];

        public List<string> FilterTimeOptions { get; } = ["Hoje", "Amanhã", "Semana", "Mês", "Total", "Personalizado"];
        public List<string> FilterModalityOptions { get; } = ["Todas", "Presencial", "Online"];
        public List<string> FilterAppointmentTypes { get; } = ["Todos", "Consulta", "Exame", "Cirurgia"];

        public DoctorAgendaViewModel()
        {
            scheduleService = new();

            SelectedFilterTime = FilterTimeOptions[0];
            SelectedModalityFilter = FilterModalityOptions[0];
            SelectedAppointmentTypeFilter = FilterAppointmentTypes[0];

            WeakReferenceMessenger.Default.Register<OnlineConcluded>(this, async (r, m) => await CompleteOnlineSchedule(m.Value));
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
                var user = UserHelper.GetLoggedUser();

                var resp = await scheduleService.GetScheduleAsync(id: user.IdUser, isDoctor: true, isHistoric: false, page: 0, pagesize: int.MaxValue, filterStatus: (int)ScheduleStatus.Confirmed);
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                allSchedule = resp.Response?.ToList() ?? [];
                allSchedule.RemoveAll(s => s.PendingPayment == 1);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DoctorSchedule = allSchedule
                        .Where(s => s.AppointmentDate.GetValueOrDefault().Date == DateTime.Today)
                        .OrderBy(s => s.AppointmentDate.GetValueOrDefault().Hour)
                        .ThenBy(s => s.AppointmentDate.GetValueOrDefault().Minute)
                        .ToObservableCollection();

                    this.BuildDashBoardValues();
                });

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
            finally
            {
                isLoading = false;
            }

        }

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;

            this.ClearFilters(canPopulateCollection: false);
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        private void BuildDashBoardValues()
        {
            GeneralTot = DoctorSchedule.Count;
            OnlineTot = DoctorSchedule.Count(s => s.IsOnline == 1);
            PresencialTot = GeneralTot - OnlineTot;
            RescheduleTot = DoctorSchedule.Count(s => s.Rescheduled == 1);
        }

        [RelayCommand] private void ClearFilters(bool canPopulateCollection = true)
        {
            FilterString = string.Empty;
            SelectedModalityFilter = FilterModalityOptions[0];
            SelectedAppointmentTypeFilter = FilterAppointmentTypes[0];
            FilterTimePreposition = "de";
            SelectedFilterTime = FilterTimeOptions[0];
            SelectedFilterTimeTermination = "Hoje";
            SelectedFilterTimeTerminationReschedule = "para Hoje";
            StartDateFilter = DateTime.Today;
            EndDateFilter = DateTime.Today;
            StartTimeFilter = TimeOnly.MinValue;
            EndTimeFilter = TimeOnly.MaxValue;

            if (canPopulateCollection)
            {
                DoctorSchedule = allSchedule
                        .Where(s => s.AppointmentDate.GetValueOrDefault().Date == DateTime.Today)
                        .OrderBy(s => s.AppointmentDate.GetValueOrDefault().Hour)
                        .ThenBy(s => s.AppointmentDate.GetValueOrDefault().Minute)
                        .ToObservableCollection();

                this.BuildDashBoardValues();
            }
        }

        private async Task FilterCollection(bool canChangeDashBoard = true)
        {
            if (isFiltring)
                return;
            isFiltring = true;

            if (IsReloading)
                return;

            if (!isLoading)
                await PopupHelper.PushLoadingAsync();

            await Task.Run(async () =>
            {
                var query = allSchedule
                .Where(s =>
                {
                    var appntTime = TimeOnly.FromDateTime(s.AppointmentDate.GetValueOrDefault());
                    return appntTime.IsBetween(StartTimeFilter, EndTimeFilter);
                })
                .Where(s =>
                {
                    TimeOptions filterTimeIndex = (TimeOptions)FilterTimeOptions.IndexOf(SelectedFilterTime);
                    var date = s.AppointmentDate.GetValueOrDefault().Date;
                    var culture = Master.Culture;
                    var calendar = culture.Calendar;
                    var today = DateTime.Today;

                    switch (filterTimeIndex)
                    {
                        case TimeOptions.Today:
                            return date == today;
                        case TimeOptions.Tomorroy:
                            return date == today.AddDays(1).Date;
                        case TimeOptions.ThisWeek:
                            var todayweek = calendar.GetWeekOfYear(today, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
                            var dateweek = calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
                            return today.Year == date.Year && todayweek == dateweek;
                        case TimeOptions.ThisMonth:
                            return date.Month == today.Month;
                        case TimeOptions.All:
                            return true;
                        case TimeOptions.Custom:
                            bool isvalid = date >= StartDateFilter.Date && date <= EndDateFilter.Date;
                            return isvalid;
                    }

                    return false;
                })
                .ToList();

                string normalizedFilterString = FilterString.Trim().ToLower();

                if (!string.IsNullOrWhiteSpace(normalizedFilterString))
                {
                    query = query
                        .Where(s => s.UserName.Contains(normalizedFilterString, StringComparison.InvariantCultureIgnoreCase)
                        || s.AppointmentTitle.Contains(normalizedFilterString, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }

                int modalityIndex = FilterModalityOptions.IndexOf(SelectedModalityFilter);
                if (modalityIndex > 0)
                {
                    query = query
                        .Where(s => s.IsOnline + 1 == modalityIndex)
                        .ToList();
                }

                int appntType = FilterAppointmentTypes.IndexOf(SelectedAppointmentTypeFilter);
                if (appntType > 0)
                {
                    query = query
                        .Where(s => s.AppointmentType == appntType)
                        .ToList();
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DoctorSchedule = query
                    .OrderBy(s => s.AppointmentDate.GetValueOrDefault().Hour)
                    .ThenBy(s => s.AppointmentDate.GetValueOrDefault().Minute)
                    .ToObservableCollection();

                    if (canChangeDashBoard)
                        this.BuildDashBoardValues();
                });
            });

            await PopupHelper.PopAllPopUpAsync();
            isFiltring = false;
        }

        [RelayCommand] private async Task FilterByString() => await FilterCollection(canChangeDashBoard: false);
        partial void OnStartDateFilterChanged(DateTime value) => _ = FilterCollection();
        partial void OnEndDateFilterChanged(DateTime value) => _ = FilterCollection();
        partial void OnStartTimeFilterChanged(TimeOnly value) => _ = FilterCollection(canChangeDashBoard: false);
        partial void OnEndTimeFilterChanged(TimeOnly value) => _ = FilterCollection(canChangeDashBoard: false);
        partial void OnSelectedAppointmentTypeFilterChanged(string value) => _ = FilterCollection(canChangeDashBoard: false);
        partial void OnSelectedModalityFilterChanged(string value) => _ = FilterCollection(canChangeDashBoard: false);

        partial void OnSelectedFilterTimeChanged(string? oldValue, string newValue)
        {
            if (IsReloading)
                return;

            int index = FilterTimeOptions.IndexOf(newValue);
            if (index < 0)
                return;

            SelectedFilterTimeTermination = index == 4 || index == 5 ? string.Empty : newValue;

            FilterTimePreposition = index switch
            {
                2 => "desta",
                3 => "deste",
                4 => string.Empty,
                5 => "entre",
                _ => "de"
            };

            SelectedFilterTimeTerminationReschedule = index switch
            {
                0 => "para Hoje",
                1 => "para Amanhã",
                2 => "para esta Semana",
                3 => "para este Mês",
                _ => string.Empty
            };

            int oldIndex = FilterTimeOptions.IndexOf(oldValue ?? string.Empty);
            if (((oldIndex == 0 && index == 5) || (oldIndex == 5 && index == 0)) 
                && StartDateFilter.Date == DateTime.Today.Date
                && EndDateFilter.Date == DateTime.Today.Date)
                return;

            _ = FilterCollection();
        }

        [RelayCommand]
        private async Task ShowDetails(ScheduleDto schedule)
        {
            await PopupHelper<ScheduleDto>.PushInstanceAsync<ScheduleDetailsPopup>(schedule);
        }

        [RelayCommand] private async Task CompleteSchedule(ScheduleDto schedule)
        {
            var result = await PopupHelper<ScheduleDto>.PushInstanceAsync<CompleteSchedulePopup>(schedule);
            if (!result)
                return;

            try
            {
                await PopupHelper.PushLoadingAsync();

                var existingSchedule = allSchedule.FirstOrDefault(s => s.IdSchedule == schedule.IdSchedule);
                if (existingSchedule is null)
                    return;

                var medicalReport = PopupHelper<string>.GetValue();
                if (string.IsNullOrWhiteSpace(medicalReport))
                    throw new Exception("Preencha o laudo para finalizar este atendimento!");

                int? oldstatus = existingSchedule.Status;
                existingSchedule.Status = (int)ScheduleStatus.Concluded;
                existingSchedule.MedicalReport = medicalReport;

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(existingSchedule, (int)UserType.Doctor, string.Empty, oldstatus));
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                allSchedule.Remove(existingSchedule);
                DoctorSchedule.Remove(schedule);
                this.BuildDashBoardValues();

                WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(DoctorPage.Agenda.ToString()));
                await Messenger.ShowToastMessage("Atendimento Finalizado");
            }
            catch (OperationCanceledException) { }
            catch(Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        private async Task CompleteOnlineSchedule(ScheduleDto schedule)
        {
            await Task.Delay(250);

            if (!await Messenger.ShowQuestionMessage($"A Consulta Online com {schedule.UserName} foi concluída?", "Conclusão de atendimento online"))
                return;

            try
            {
                await PopupHelper.PushLoadingAsync();
                var index = DoctorSchedule.IndexOf(schedule);

                schedule.CallConcluded = 1;

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(schedule, userTypeUpd:(int)UserType.Doctor, userNameUpd: schedule.DoctorName ?? string.Empty));
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }
                
                if (index >= 0)
                {
                    DoctorSchedule[index] = schedule;
                }

                await PopupHelper.PopLoadingAsync();
                await CompleteSchedule(schedule);
            }
            catch (OperationCanceledException)
            {
                await PopupHelper.PopAllPopUpAsync();
            }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
                await PopupHelper.PopAllPopUpAsync();
            }
        }

        [RelayCommand] private async Task ShowOnlinePage(ScheduleDto schedule)
        {
            if (schedule.CallConcluded == 1)
            {
                if (!await Messenger.ShowQuestionMessage("Deseja mesmo entrar online novamente? Você já marcou essa consulta como concluída anteriormente.","Reconectar"))
                    return;
            }

            WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(DoctorPage.Agenda.ToString()));
            await Navigator.PushOnlinePage(schedule);
        }

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
                dto.FeedBack = "[CANCELADO PELO(A) MÉDICO(A)] " + PopupHelper<string>.GetValue();

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(dto, (int)UserType.Doctor, string.Empty, oldstatus));
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

            await Messenger.ShowMessage("Consulta reagendada\n\nUma notificação foi enviada para o paciente!", "Sucesso");
            await this.ReloadPage();

            WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(DoctorPage.Agenda.ToString()));
        }
    }
}
