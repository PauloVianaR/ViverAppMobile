using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Globalization;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using AppointmentType = ViverAppMobile.Models.AppointmentType;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Patient
{
    public partial class PatientScheduleViewModel : ObservableObject, IViewModelInstancer
    {
        [ObservableProperty] private DateTime? scheduleDateSelected = DateTime.Today;
        [ObservableProperty] private DateTime minimumDate = DateTime.Today;
        [ObservableProperty] private AppointmentType selectedAppointmentType = AppointmentType.Consultation;
        [ObservableProperty] private string selectedAppointment = "Selecione o tipo de consulta";
        [ObservableProperty] private string selectedAppointmentTypeTitle = "Consulta";
        [ObservableProperty] private string appointmentPrice = "R$ 0.00";
        [ObservableProperty] private string appointmentDuration = "0h00min";
        [ObservableProperty] private string appointmentDescription = string.Empty;
        [ObservableProperty] private string selectedDoctorName = string.Empty;
        [ObservableProperty] private string scheduleNotes = string.Empty;
        [ObservableProperty] private string selectedModality;
        [ObservableProperty] private string scheduleTimeSelected;
        [ObservableProperty] private bool isConsultation = true;
        [ObservableProperty] private bool canShowPriceAndDuration = false;
        [ObservableProperty] private bool isOnline = false;
        [ObservableProperty] private bool canScheduleAppointment = false;
        [ObservableProperty] private bool isRefreshing = false;
        [ObservableProperty] private int selectedDoctorId = 0;
        [ObservableProperty] private int appointmentPickerIndex = 0;

        private IEnumerable<Appointment> appointments = [];
        private List<DoctorDto> doctors = [];
        private IEnumerable<AvailabilityClinic> availabilitiesClinic = [];
        private IEnumerable<AvailabilityDoctor> availabilitiesDoctors = [];
        private IEnumerable<Config> configs = [];
        private IEnumerable<ScheduleDto> schedulePendingOrConfirmed = [];
        private IEnumerable<Holiday> holidays = [];

        private readonly AppointmentService appointmentService;
        private readonly UserService userService;
        private readonly AvailabilityClinicService availabilityClinicService;
        private readonly AvailabilityDoctorService availabilityDoctorService;
        private readonly HolidayService holidayService;
        private readonly ConfigService configService;
        private readonly ScheduleService scheduleService;
        private UserDto? user;
        private Scheduler scheduler = null!;

        public ObservableCollection<SelectableModel<DoctorDto>> FilteredDoctors { get; set; } = [];
        public ObservableCollection<string> FilteredAppointments { get; set; } = [];
        public ObservableCollection<string> TimesCanSchedule { get; set; } = ["Selecione um horário"];
        public ObservableCollection<DateTime> UnavailableDates { get; set; } = [];
        public Func<DateTime, bool> CanSelectDate { get; private set; }
        public DateTime MaximumDate => DateTime.Today.AddYears(1);

        public PatientScheduleViewModel()
        {
            CanSelectDate = _ => false;

            ScheduleTimeSelected = TimesCanSchedule[0];
            SelectedModality = "Presencial";

            appointmentService = new();
            availabilityDoctorService = new();
            availabilityClinicService = new();
            holidayService = new();
            configService = new();
            scheduleService = new();
            userService = new();
        }

        public async Task InitializeAsync()
        {
            WeakReferenceMessenger.Default.Register<ShowSchedulePageSelectAppointmentByMainMessage>(this, (r, m) => SelectAppointmentByExternalCall(m.Value));

            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() => this.BlockAllDays());

            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                var loggedUser = UserHelper.GetLoggedUser();
                if (loggedUser is null)
                {
                    await Navigator.RedirectToMainPage();
                    return "Falha ao carregar dados do usuário";
                }

                await MainThread.InvokeOnMainThreadAsync(() => user = loggedUser);

                var appointmentResp = await appointmentService.GetAllAsync();
                if (!appointmentResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    appointmentResp.ThrowIfIsNotSucess();
                }

                var appointmentsList = appointmentResp.Response ?? [];
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    appointments = appointmentsList;
                    this.FilterAppointmentsCollection();
                });

                var doctorResp = await userService.GetDoctorsAsync(getBlocked: false, getRejected: false, getPendingApproval: false);
                if (!doctorResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    doctorResp.ThrowIfIsNotSucess();
                }

                doctors = doctorResp?.Response?.ToList() ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    FilteredDoctors.Clear();
                    doctors.ForEach(d => FilteredDoctors.Add(new SelectableModel<DoctorDto>(d, d.IdUser, SelectedDoctorId)));
                });

                var avDocResp = await availabilityDoctorService.GetAllAsync();
                if (!avDocResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    avDocResp.ThrowIfIsNotSucess();
                }

                await MainThread.InvokeOnMainThreadAsync(() => availabilitiesDoctors = avDocResp?.Response ?? []);

                var avClinicResp = await availabilityClinicService.GetAllAsync();
                if (!avClinicResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    avClinicResp.ThrowIfIsNotSucess();
                }

                await MainThread.InvokeOnMainThreadAsync(() => availabilitiesClinic = avClinicResp.Response ?? []);

                var configResp = await configService.GetAllAsync();
                if (!configResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    configResp.ThrowIfIsNotSucess();
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    configs = configResp?.Response ?? [];
                });

                int? minConsultationTime = configs?.FirstOrDefault(c => c?.Idconfig == (int?)ConfigType.AverageConsultationTimeMin).Value ?? 20;
                int? minExaminationTime = configs?.FirstOrDefault(c => c?.Idconfig == (int?)ConfigType.AverageExaminationTimeMin).Value ?? 30;
                int? minSurgeryTime = configs?.FirstOrDefault(c => c?.Idconfig == (int?)ConfigType.AverageSurgeryTimeMin).Value ?? 60;
                int? configInterval = configs?.FirstOrDefault(c => c?.Idconfig == (int?)ConfigType.AppointmentIntervalMin).Value ?? 10;
                TimeOnly intervalMinAppointment = new(0, configInterval ?? 0);

                var scheduleResp = await scheduleService.GetScheduleAsync(0, isDoctor: default, isHistoric: false, page: 0, pagesize: int.MaxValue);
                if (!scheduleResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    scheduleResp.ThrowIfIsNotSucess();
                }

                await MainThread.InvokeOnMainThreadAsync(() => schedulePendingOrConfirmed = scheduleResp.Response ?? []);

                var holidayResp = await holidayService.GetAllAsync();
                if (!holidayResp.WasSuccessful)
                    throw new Exception($"Falha ao calcular os feriados cadastrados para calcular os dias disponíveis para agendamento.\nErro:{holidayResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                await MainThread.InvokeOnMainThreadAsync(() => holidays = holidayResp?.Response ?? []);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    scheduler = new(availabilitiesDoctors, availabilitiesClinic, schedulePendingOrConfirmed, holidays, minConsultationTime ?? 20, minExaminationTime ?? 30, minSurgeryTime ?? 60, intervalMinAppointment);
                });

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
                await MainThread.InvokeOnMainThreadAsync(() => ScheduleTimeSelected = TimesCanSchedule[0]);
            }
        }


        private void LoadUnavailableDates(int iddoctor)
        {
            UnavailableDates.Clear();
            ScheduleDateSelected = DateTime.Today;

            var docAvailabilities = availabilitiesDoctors
                .Where(a => a.Iddoctor == iddoctor);

            var unDays = scheduler.GetUnavailableDays(iddoctor,IsOnline, ScheduleDateSelected ?? DateTime.MinValue);
            unDays.ForEach(d => UnavailableDates.Add(d));

            CanSelectDate = date =>
            {
                if (date < DateTime.Today || date > DateTime.Today.AddYears(1))
                    return false;

                if (UnavailableDates.Contains(date.Date))
                    return false;

                return true;
            };
            
            ScheduleDateSelected = scheduler.GetFirstValidDay(iddoctor,IsOnline, ScheduleDateSelected ?? DateTime.MinValue);
            MinimumDate = ScheduleDateSelected ?? DateTime.Today;
            OnPropertyChanged(nameof(CanSelectDate));
        }

        private void LoadAvailableTimesToSchedule(DateTime? dateToFind)
        {
            CleanTimesCanScheduleList();

            var avDoc = availabilitiesDoctors.FirstOrDefault(a => a.Daytype == (int?)dateToFind?.DayOfWeek 
            && (a.Isonline == (sbyte)1) == IsOnline
            && a.Iddoctor == SelectedDoctorId);
            if (avDoc is null) return;

            var avClinic = availabilitiesClinic.FirstOrDefault(c => c.Daytype == (int?)dateToFind?.DayOfWeek);
            if (avClinic is null) return;

            var appnt = appointments.FirstOrDefault(a => a.Title == SelectedAppointment);
            if (appnt is null) return;

            var validTimes = scheduler.GetDayAvailableTimes(dateToFind ?? DateTime.MinValue, appnt, avDoc, avClinic);
            if (validTimes is null || validTimes.Count == 0) return;

            foreach (var t in validTimes)
            {
                TimesCanSchedule.Add($"{t.Start:HH:mm} - {t.End:HH:mm}");
            }
        }

        private void CleanTimesCanScheduleList()
        {
            TimesCanSchedule.Clear();
            TimesCanSchedule.Add("Selecione um horário");
            ScheduleTimeSelected = TimesCanSchedule[0];
            CanScheduleAppointment = false;
        }

        public void BlockAllDays()
        {
            UnavailableDates.Clear();
            CleanTimesCanScheduleList();

            CanSelectDate = _ => false;
            OnPropertyChanged(nameof(CanSelectDate));
        }

        private void FilterAppointmentsCollection()
        {
            FilteredAppointments.Clear();

            SelectedAppointmentTypeTitle = SelectedAppointmentType switch
            {
                AppointmentType.Consultation => "Consulta",
                AppointmentType.Examination => "Exame",
                AppointmentType.Surgery => "Cirurgia",
                _ => "Consulta",
            };

            FilteredAppointments.Add($"Selecione o tipo de {SelectedAppointmentTypeTitle.ToLower()}");

            appointments.ToList().ForEach(item =>
            {
                if (item.Idappointmenttype == (int)SelectedAppointmentType)
                    FilteredAppointments.Add(item?.Title ?? string.Empty);
            });

            SelectedAppointment = FilteredAppointments[0];
        }

        [RelayCommand] private void SetIsOnline()
        {
            IsOnline = true;
            SelectedModality = "Online";
        }

        [RelayCommand] private void SetIsNotOnline()
        {
            IsOnline = false;
            SelectedModality = "Presencial";
        }

        [RelayCommand] private void SetAppointmentType(string parameter)
        {
            if (!Enum.TryParse(parameter, out AppointmentType aptype))
                return;

            IsConsultation = aptype == AppointmentType.Consultation;

            SelectedAppointmentType = aptype;
            this.FilterAppointmentsCollection();
        }

        [RelayCommand] private void SetSelectedDoctorId(int id)
        {
            SelectedDoctorId = id;
            var doc = doctors.FirstOrDefault(d => d.IdUser == id);
            if (doc is null)
                return;

            SelectedDoctorName = $"{doc.Title} {doc.Name}";

            foreach (var d in FilteredDoctors)
            {
                d.SelectedId = SelectedDoctorId;
            }

            this.VerifyCanSelectDate();
        }

        partial void OnIsOnlineChanged(bool value)
        {
            SelectedDoctorId = 0;

            var filtered = value
                ? doctors.Where(doc => doc.Attendonline == (sbyte)1)
                : doctors;

            FilteredDoctors.Clear();
            foreach (var doc in filtered)
                FilteredDoctors.Add(new SelectableModel<DoctorDto>(doc, doc.IdUser, SelectedDoctorId));

            this.BlockAllDays();
        }

        partial void OnSelectedAppointmentTypeChanged(AppointmentType oldValue, AppointmentType newValue)
        {
            if (oldValue == newValue)
                return;

            IsOnline = false;
            SelectedModality = "Presencial";
        }

        partial void OnAppointmentPickerIndexChanged(int value)
        {
            CanShowPriceAndDuration = value > 0;
        }

        partial void OnSelectedAppointmentChanged(string value)
        {
            this.VerifyCanSelectDate();
            this.CleanTimesCanScheduleList();

            if (!CanShowPriceAndDuration)
                return;

            var selAppnt = appointments.FirstOrDefault(a => a.Title == value);
            if (selAppnt is null)
                return;

            AppointmentDescription = selAppnt?.Description ?? string.Empty;
            AppointmentPrice = selAppnt.Price?.ToString("C2", Master.Culture) ?? string.Empty;
            AppointmentDuration = string.Concat(selAppnt.Averagetime?.Hour.ToString() ?? "0", "h", selAppnt.Averagetime?.Minute.ToString("D2") ?? "00", "min");
        }

        private void VerifyCanSelectDate()
        {
            if (string.IsNullOrEmpty(SelectedAppointment))
                return;

            if (SelectedDoctorId <= 0 || SelectedAppointment.Contains("Selecione"))
            {
                BlockAllDays();
                return;
            }

            LoadUnavailableDates(SelectedDoctorId);
        }

        partial void OnScheduleDateSelectedChanged(DateTime? value)
        {
            LoadAvailableTimesToSchedule(value);
        }

        partial void OnScheduleTimeSelectedChanged(string value)
        {
            if (string.IsNullOrEmpty(ScheduleTimeSelected))
                return;

            if (ScheduleTimeSelected.Contains("Selecione"))
                return;

            CanScheduleAppointment = true;
        }

        [RelayCommand] private async Task ConfirmSchedule()
        {
            if (!CanScheduleAppointment)
                return;

            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                await PopupHelper.PushLoadingAsync();

                var appnt = appointments.FirstOrDefault(a => a.Title == SelectedAppointment);
                if (appnt is null)
                    return;

                var selectedTime = ScheduleTimeSelected.Split('-').GetValue(0).ToString().Trim();
                var selectedDateTime = new DateTime(DateOnly.FromDateTime(ScheduleDateSelected.GetValueOrDefault()), TimeOnly.Parse(selectedTime));

                var resp = await scheduleService.CreateAsync(new Schedule
                {
                    Idappointment = appnt.Idappointment,
                    Iduser = user.IdUser,
                    Iddoctor = SelectedDoctorId,
                    Idclinic = 1,
                    Status = (int)ScheduleStatus.Pending,
                    Appointmentdate = selectedDateTime,
                    Obs = ScheduleNotes,
                    Isonline = IsOnline ? (sbyte)1 : (sbyte)0,
                    Rescheduled = 0,
                    Originaldate = selectedDateTime,
                    Pendingpayment = (sbyte)1
                });

                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(resp.ResponseErr);
                }

                WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(PatientPage.Schedule.ToString()));

                bool userWouldLikePayNow = await Messenger.ShowQuestionMessage("Sua consulta foi agendada com sucesso.\nGostaria de já realizar seu pagamento ou confirmar que irá pagar na hora?\n\nVocê ainda terá 24hrs para confirmar ou pagar a consulta antes que ela seja cancelada. 😊", "Sucesso. Deseja pagar/confirmar agora?");

                if (userWouldLikePayNow)
                {
                    WeakReferenceMessenger.Default.Send(new ShowPaymentPageSelectScheduleToPayMessage(resp.Response.Idschedule));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
            await this.ReloadPage();
        }

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsRefreshing)
                return;
            IsRefreshing = true;

            appointments.ToList().Clear();
            doctors.ToList().Clear();
            availabilitiesClinic.ToList().Clear();
            availabilitiesDoctors.ToList().Clear();
            configs.ToList().Clear();
            schedulePendingOrConfirmed.ToList().Clear();
            holidays.ToList().Clear();
            UnavailableDates.Clear();
            this.CleanTimesCanScheduleList();
            FilteredAppointments.Clear();
            FilteredDoctors.Clear();
            CanScheduleAppointment = false;

            await Loader.RunWithLoadingAsync(LoadAllAsync);

            ScheduleDateSelected = DateTime.Today;
            AppointmentPrice = "R$ 0.00";
            AppointmentDuration = "0h00min";
            SelectedDoctorName = string.Empty;
            ScheduleNotes = string.Empty;
            ScheduleTimeSelected = TimesCanSchedule[0];
            SelectedModality = "Presencial";
            IsConsultation = SelectedAppointmentType == AppointmentType.Consultation;
            CanShowPriceAndDuration = false;
            IsOnline = false;
            CanScheduleAppointment = false;
            IsRefreshing = false;
            SelectedDoctorId = 0;
            AppointmentPickerIndex = 0;

            foreach (var item in FilteredDoctors)
            {
                item.SelectedId = SelectedDoctorId;
            }

            IsRefreshing = false;
        }

        private void SelectAppointmentByExternalCall(int _idappointment)
        {
            var appnt = appointments.FirstOrDefault(a => a.Idappointment == _idappointment);
            if (appnt is null)
            {
                this.SetAppointmentType(AppointmentType.Consultation.ToString());
                SelectedAppointment = FilteredAppointments[0];
                CanShowPriceAndDuration = false;
            }
            else
            {
                var type = (AppointmentType)appnt.Idappointmenttype;
                this.SetAppointmentType(type.ToString());
                SelectedAppointment = appnt.Title ?? FilteredAppointments[0];
                OnSelectedAppointmentChanged(appnt.Title ?? string.Empty);
            }
        }
    }
}
