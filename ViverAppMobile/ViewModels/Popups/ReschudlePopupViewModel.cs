using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Services;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Popups
{
    public partial class ReschudlePopupViewModel : ObservableObject, IViewModelInstancer
    {
        [ObservableProperty] private DateTime? scheduleDateSelected = DateTime.Today;
        [ObservableProperty] private DateTime minimumDate = DateTime.Today;
        [ObservableProperty] private bool canReschedule = false;
        [ObservableProperty] private string scheduleTimeSelected;

        private IEnumerable<AvailabilityClinic> availabilitiesClinic = [];
        private IEnumerable<AvailabilityDoctor> availabilitiesDoctors = [];

        private readonly AppointmentService appointmentService;
        private readonly AvailabilityClinicService availabilityClinicService;
        private readonly AvailabilityDoctorService availabilityDoctorService;
        private readonly HolidayService holidayService;
        private readonly ConfigService configService;
        private readonly ScheduleService scheduleService;
        private readonly UserDto? user;
        private Appointment? selectedAppointment;
        private Scheduler scheduler = null!;
        private bool isLoadingDate = true;

        public ObservableCollection<string> TimesCanSchedule { get; set; } = ["Selecione um horário"];
        public ObservableCollection<DateTime> UnavailableDates { get; set; } = [];
        
        public DateTime MaximumDate => DateTime.Today.AddYears(1);
        public Func<DateTime, bool> CanSelectDate { get; private set; }
        public ScheduleDto? UserSchedule { get; }

        public ReschudlePopupViewModel()
        {
            appointmentService = new();
            availabilityClinicService = new();
            availabilityDoctorService = new();
            holidayService = new();
            configService = new();
            scheduleService = new();
            CanSelectDate = _ => false;

            ScheduleTimeSelected = TimesCanSchedule[0];
            UserSchedule = PopupHelper<ScheduleDto>.GetValue();
            user = UserHelper.GetLoggedUser();
        }

        public async Task InitializeAsync()
        {
            await Loader.RunWithLoadingAsync(LoadAllAsync, ispopup:true);
        }

        public async Task<string?> LoadAllAsync()
        {
            try
            {
                StringBuilder sb = new();

                if (user is null)
                {
                    await Messenger.ShowErrorMessageAsync("Falha ao carregar dados do usuário");
                    await Navigator.RedirectToMainPage();
                    return null;
                }

                var appntResp = await appointmentService.GetByIdAsync(UserSchedule.IdAppointment);
                if (!appntResp.WasSuccessful)
                    sb.AppendLine($"Falha ao carregar dados do tipo de consulta.\nErro:{appntResp.Response}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                await MainThread.InvokeOnMainThreadAsync(() => selectedAppointment = appntResp.Response);

                var avDocResp = await availabilityDoctorService.GetAllAsync();
                if (!avDocResp.WasSuccessful)
                    sb.AppendLine($"Falha ao carregar disponibilidade de agenda dos médicos.\nErro:{avDocResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                await MainThread.InvokeOnMainThreadAsync(() => availabilitiesDoctors = avDocResp?.Response ?? []);

                var avClinicResp = await availabilityClinicService.GetAllAsync();
                if (!avClinicResp.WasSuccessful)
                    sb.AppendLine($"Falha ao carregar a disponibilidade da clínica.\nErro:{avClinicResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                await MainThread.InvokeOnMainThreadAsync(() => availabilitiesClinic = avClinicResp.Response ?? []);

                var configResp = await configService.GetAllAsync();
                if (!configResp.WasSuccessful)
                    sb.AppendLine($"Falha ao carregar as configurações internas do sistema.\nErro:{configResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                var configs = configResp?.Response ?? [];
                int minConsultationTime = configs.FirstOrDefault(c => c.Idconfig == (int)ConfigType.AverageConsultationTimeMin).Value ?? 20;
                int minExaminationTime = configs.FirstOrDefault(c => c.Idconfig == (int)ConfigType.AverageExaminationTimeMin).Value ?? 30;
                int minSurgeryTime = configs.FirstOrDefault(c => c.Idconfig == (int)ConfigType.AverageSurgeryTimeMin).Value ?? 60;
                int configInterval = configs.FirstOrDefault(c => c.Idconfig == (int)ConfigType.AppointmentIntervalMin).Value ?? 10;
                TimeOnly intervalMinAppointment = new(0, configInterval);

                var scheduleResp = await scheduleService.GetScheduleAsync(0, isDoctor: default, isHistoric: false, page: 0, pagesize: int.MaxValue);
                if (!scheduleResp.WasSuccessful)
                    sb.AppendLine($"Falha ao carregar a agenda.\nErro:{scheduleResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                var schedulePendingOrConfirmed = scheduleResp.Response ?? [];

                var holidayResp = await holidayService.GetAllAsync();
                if (!holidayResp.WasSuccessful)
                    throw new Exception($"Falha ao calcular os feriados cadastrados para calcular os dias disponíveis para agendamento.\nErro:{holidayResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                var holidays = holidayResp?.Response ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    scheduler = new(availabilitiesDoctors, availabilitiesClinic, schedulePendingOrConfirmed, holidays, minConsultationTime, minExaminationTime, minSurgeryTime, intervalMinAppointment);
                });

                string errors = sb.ToString();

                if (!string.IsNullOrEmpty(errors))
                    throw new Exception(sb.ToString());

                await MainThread.InvokeOnMainThreadAsync(() => LoadUnavailableDates());
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

        private void LoadUnavailableDates()
        {
            int iddoctor = UserSchedule.Iddoctor;
            bool isOnline = UserSchedule.IsOnline == (sbyte)1;

            var docAvailabilities = availabilitiesDoctors
                .Where(a => a.Iddoctor == iddoctor);

            var unDays = scheduler.GetUnavailableDays(iddoctor, isOnline, ScheduleDateSelected ?? DateTime.MinValue);
            unDays.ForEach(d => UnavailableDates.Add(d));

            CanSelectDate = date =>
            {
                if (date < DateTime.Today || date > DateTime.Today.AddYears(1))
                    return false;

                if (UnavailableDates.Contains(date.Date))
                    return false;

                return true;
            };

            isLoadingDate = false;
            ScheduleDateSelected = UserSchedule.AppointmentDate;
            MinimumDate = scheduler.GetFirstValidDay(iddoctor, isOnline, DateTime.Today);
            OnPropertyChanged(nameof(CanSelectDate));
        }

        partial void OnScheduleDateSelectedChanged(DateTime? value)
        {
            if (isLoadingDate)
                return;

            LoadAvailableTimesToSchedule(value);
        }

        private void LoadAvailableTimesToSchedule(DateTime? dateToFind)
        {
            TimesCanSchedule.Clear();
            TimesCanSchedule.Add("Selecione um horário");
            ScheduleTimeSelected = TimesCanSchedule[0];

            bool isOnline = UserSchedule.IsOnline == (sbyte)1;

            var avDoc = availabilitiesDoctors.FirstOrDefault(a => a.Daytype == (int?)dateToFind?.DayOfWeek 
            && (a.Isonline == (sbyte)1) == isOnline
            && a.Iddoctor == UserSchedule.Iddoctor);
            if (avDoc is null) return;

            var avClinic = availabilitiesClinic.FirstOrDefault(c => c.Daytype == (int?)dateToFind?.DayOfWeek);
            if (avClinic is null) return;

            var validTimes = scheduler.GetDayAvailableTimes(dateToFind ?? DateTime.MinValue, selectedAppointment ?? new() { Averagetime = new TimeOnly(0,20)}, avDoc, avClinic);

            if (validTimes is null || validTimes.Count == 0) return;

            foreach (var t in validTimes)
            {
                TimesCanSchedule.Add($"{t.Start:HH:mm} - {t.End:HH:mm}");
            }

            DateTime appntDate = UserSchedule.AppointmentDate ?? DateTime.MinValue;
            var timeSchedule = TimesCanSchedule.FirstOrDefault(t => appntDate.ToString("HH:mm").Contains(t.Split('-').GetValue(0).ToString().Trim()));

            if (timeSchedule is not null)
            {
                ScheduleTimeSelected = timeSchedule;
                return;
            }

            ScheduleTimeSelected = TimesCanSchedule[0];
        }

        partial void OnScheduleTimeSelectedChanged(string value)
        {
            if (scheduleDateSelected == DateTime.Today || ScheduleDateSelected == DateTime.MinValue)
                return;

            if (string.IsNullOrWhiteSpace(ScheduleTimeSelected))
                return;

            if (ScheduleTimeSelected.Contains("Selecione") || !ScheduleTimeSelected.Contains('-')
                || DateOnly.FromDateTime(ScheduleDateSelected ?? DateTime.MinValue) 
                == DateOnly.FromDateTime(UserSchedule.AppointmentDate ?? DateTime.MinValue))
                return;

            var selectedTime = ScheduleTimeSelected.Split('-').GetValue(0).ToString().Trim();

            if (TimeOnly.TryParse(selectedTime, out TimeOnly newTime)
                && newTime == TimeOnly.FromDateTime(UserSchedule.AppointmentDate ?? DateTime.MinValue))
                return;

            CanReschedule = true;
        }

        [RelayCommand] private async Task Reschedule()
        {
            if (!CanReschedule)
                return;

            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                await PopupHelper.PushLoadingAsync();

                var selectedTime = ScheduleTimeSelected.Split('-').GetValue(0).ToString().Trim();
                var selectedDateTime = new DateTime(DateOnly.FromDateTime(ScheduleDateSelected.GetValueOrDefault()), TimeOnly.Parse(selectedTime));

                if (UserSchedule is null)
                    return;

                int? status = (int)ScheduleStatus.Rescheduled;
                UserSchedule.AppointmentDate = selectedDateTime;
                UserSchedule.Rescheduled = 1;

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(UserSchedule, user.Usertype,user.Name ?? "USUÁRIO",status));
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

            await PopupHelper.PopLastPopUpAsync();
        }
    }
}
