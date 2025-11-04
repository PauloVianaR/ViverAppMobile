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
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Doctor
{
    public partial class DoctorHomeViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly ScheduleService scheduleService;
        private readonly DoctorPropsService doctorPropsService;

        [ObservableProperty] private int scheduleToday = 0;
        [ObservableProperty] private int schedulePresencial = 0;
        [ObservableProperty] private int scheduleOnline = 0;
        [ObservableProperty] private int scheduleThisWeek = 0;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private DoctorDto loggedDoctor = null!;
        [ObservableProperty] private ObservableCollection<ScheduleDto> confirmedTodaySchedule = [];

        public string TodayWeek => DateTime.Today.ToString("D", Master.Culture);

        public DoctorHomeViewModel()
        {
            scheduleService = new();
            doctorPropsService = new();

            WeakReferenceMessenger.Default.Register<OnlineConcluded>(this, async (r, m) => await CompleteOnlineSchedule(m.Value));
        }

        public async Task InitializeAsync()
        {
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            if (IsLoading)
                return null;
            IsLoading = true;

            try
            {
                var user = UserHelper.GetLoggedUser() ?? throw new Exception("Falha ao carregar dados do usuário logado.\nTente fazer o login novamente!");
                var dpResp = await doctorPropsService.GetByIdAsync(user.IdUser);
                if (!dpResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    dpResp.ThrowIfIsNotSucess();
                }

                if (dpResp.Response is null)
                    throw new Exception("Falha ao carregar dados do usuário logado.\nTenta fazer o login novamente!");

                await MainThread.InvokeOnMainThreadAsync(() => LoggedDoctor = new(user, dpResp.Response));

                var scheduleResp = await scheduleService.GetScheduleAsync(LoggedDoctor.IdUser, isDoctor: true, isHistoric: false, page: 0, pagesize: int.MaxValue, filterStatus: (int)ScheduleStatus.Confirmed);
                if (!scheduleResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    scheduleResp.ThrowIfIsNotSucess();
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var allSchedule = scheduleResp?.Response?.ToArray() ?? [];
                    ScheduleToday = allSchedule.Count(s => s.AppointmentDate.GetValueOrDefault().Date == DateTime.Today.Date);
                    SchedulePresencial = allSchedule.Count(s => s.AppointmentDate.GetValueOrDefault().Date == DateTime.Today.Date && s.IsOnline == 0);
                    ScheduleOnline = ScheduleToday - SchedulePresencial;

                    var culture = Master.Culture;
                    var calendar = culture.Calendar;
                    var today = DateTime.Today;
                    var todayweek = calendar.GetWeekOfYear(today, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);

                    ScheduleThisWeek = allSchedule.Count(s =>
                    {
                        var date = s.AppointmentDate.GetValueOrDefault().Date;
                        var dateweek = calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);

                        return today.Year == date.Year && todayweek == dateweek;
                    });

                    ConfirmedTodaySchedule = allSchedule
                        .Where(s => s.AppointmentDate.GetValueOrDefault().Date == DateTime.Today.Date)
                        .OrderBy(s => s.AppointmentDate.GetValueOrDefault().Hour)
                        .ThenBy(s => s.AppointmentDate.GetValueOrDefault().Minute)
                        .ToObservableCollection();
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
                IsLoading = false;
            }
        }

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        [RelayCommand] private async Task ShowOnlinePage(ScheduleDto schedule)
        {
            if (schedule.CallConcluded == 1)
            {
                if (!await Messenger.ShowQuestionMessage("Deseja mesmo entrar online novamente? Você já marcou essa consulta como concluída anteriormente.", "Reconectar"))
                    return;
            }

            WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(DoctorPage.Home.ToString()));
            await Navigator.PushOnlinePage(schedule);
        }

        [RelayCommand] private async Task ShowScheduleDetails(ScheduleDto schedule)
        {
            await PopupHelper<ScheduleDto>.PushInstanceAsync<ScheduleDetailsPopup>(schedule);
        }


        [RelayCommand]
        private async Task CompleteSchedule(ScheduleDto schedule)
        {
            var result = await PopupHelper<ScheduleDto>.PushInstanceAsync<CompleteSchedulePopup>(schedule);
            if (!result)
                return;

            try
            {
                await PopupHelper.PushLoadingAsync();

                var existingSchedule = ConfirmedTodaySchedule.FirstOrDefault(s => s.IdSchedule == schedule.IdSchedule);
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

                ScheduleToday--;
                ScheduleThisWeek--;

                if (existingSchedule.IsOnline == 0)
                    SchedulePresencial--;

                ScheduleOnline = ScheduleToday - SchedulePresencial;

                ConfirmedTodaySchedule.Remove(existingSchedule);

                WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(DoctorPage.Home.ToString()));
                await Messenger.ShowToastMessage("Atendimento Finalizado");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
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
                var index = ConfirmedTodaySchedule.IndexOf(schedule);

                schedule.CallConcluded = 1;

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(schedule, userTypeUpd: (int)UserType.Doctor, userNameUpd: schedule.DoctorName ?? string.Empty));
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                if (index >= 0)
                {
                    ConfirmedTodaySchedule[index] = schedule;
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

        [RelayCommand]
        private void SwitchPage(string newPage)
        {
            var docpage = Enum.TryParse(newPage, ignoreCase: true, out DoctorPage dp) ? dp : DoctorPage.Home;
            WeakReferenceMessenger.Default.Send(new NavigateTabIndex((int)docpage));
        }
    }
}
