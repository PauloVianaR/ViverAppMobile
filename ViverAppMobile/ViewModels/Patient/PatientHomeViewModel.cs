using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Patient
{
    public partial class PatientHomeViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly ScheduleService scheduleService;
        private readonly ClinicService clinicService;
        private readonly AppointmentService appointmentService;
        private ScheduleDto? nextAppointment;
        private UserDto? user;
        private Clinic? clinic;

        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private bool isUserPremium = false;
        [ObservableProperty] private bool isNotUserPremium = true;
        [ObservableProperty] private bool userHasNextAppointment = false;
        [ObservableProperty] private bool nextAppointmentIsPending = false;
        [ObservableProperty] private bool nextAppointmentIsOnline = false;
        [ObservableProperty] private bool nextAppointmentWasRescheduled = false;
        [ObservableProperty] private bool nextAppointmentPendingPayment = false;
        [ObservableProperty] private bool canShowAvailableSurgeries = false;
        [ObservableProperty] private bool canShowAvailableExams = false;
        [ObservableProperty] private string userName = string.Empty;
        [ObservableProperty] private string nextAppointmentTitle = string.Empty;
        [ObservableProperty] private string nextAppointmentNote = string.Empty;
        [ObservableProperty] private string nextAppointmentType = string.Empty;
        [ObservableProperty] private string nextAppointmentTypeSufix = string.Empty;
        [ObservableProperty] private string nextAppointmentTypePrefix = string.Empty;
        [ObservableProperty] private string nextAppointmentDoctorName = string.Empty;
        [ObservableProperty] private string nextAppointmentDoctorMainSpecialty = string.Empty;
        [ObservableProperty] private string nextAppointmentStatus = string.Empty;
        [ObservableProperty] private string clinicName = string.Empty;
        [ObservableProperty] private string clinicComplement = string.Empty;
        [ObservableProperty] private string clinicFullAdress = string.Empty;
        [ObservableProperty] private string clinicIcon = "\ue800";
        [ObservableProperty] private string howToGetThereIcon = "\ue80c";
        [ObservableProperty] private string howToGetThereText = "Como Chegar";
        [ObservableProperty] private string nextAppointmentDate = string.Empty;
        [ObservableProperty] private string nextAppointmentHour = string.Empty;
        [ObservableProperty] private string nextAppointmentOriginalDate = string.Empty;
        [ObservableProperty] private int nextAppointmentStatusID = 0;
        [ObservableProperty] private ObservableCollection<CarouselItem> carouselItems = [];

        public ObservableCollection<Appointment> AvailableSurgeries { get; set; } = [];
        public ObservableCollection<Appointment> AvailableExams { get; set; } = [];

        public PatientHomeViewModel()
        {
            scheduleService = new();
            clinicService = new();
            appointmentService = new();

            CarouselItems =
            [
                new(
                    image:"carouselimage1.png",
                    title:"Sua visão merece cuidado especial",
                    sub:"Exames regulares previnem problemas",
                    info:"A prevenção é o melhor remédio para manter seus olhos saudáveis por toda a vida.",
                    icon:"\ue836",
                    backcolor:Colors.CornflowerBlue,
                    margin:new(12,35,0,0)),
                new(
                    image:"carouselimage2.png",
                    title:"Tecnologia de ponta ao seu alcance",
                    sub:"Modernidade para diagnósticos precisos",
                    info:"Utilizamos os mais avançados equipamentos para cuidar da sua saúde visual.",
                    icon:"\ue809",
                    backcolor:Colors.Gold,
                    margin:new(12,23,0,0)),
                new(
                    image:"carouselimage3.png",
                    title:"Beleza natural aos seus esplêndidos olhos",
                    sub:"Cuidamos da saúde e da estética",
                    info:"Seus olhos refletem sua personalidade. Mantenha-os sempre belos e saudáveis.",
                    icon:"\ue83e",
                    backcolor:Colors.LimeGreen,
                    margin:new(12,23,0,0)),
                new(
                    image:"carouselimage4.png",
                    title:"Equipe especializada e dedicada",
                    sub:"Profissionais comprometidos com você",
                    info:"Nossa equipe médica está sempre pronta para oferecer o melhor tratamento.",
                    icon:"\ue82f",
                    backcolor:Colors.OrangeRed,
                    margin:new(12,23,0,0)),
            ];
        }

        public async Task InitializeAsync()
        {
            var loggedUser = UserHelper.GetLoggedUser();
            if (loggedUser is not null)
                user = loggedUser;

            UserName = user?.Name ?? "usuário";
            IsUserPremium = user?.IsPremium == (sbyte)1;
            IsNotUserPremium = !IsUserPremium;

            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            StringBuilder sb = new();

            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                if (IsLoading)
                    return null;

                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = true);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AvailableSurgeries.Clear();
                    AvailableExams.Clear();
                });

                Master.GlobalToken.ThrowIfCancellationRequested();

                var scheduleResp = await scheduleService.GetScheduleAsync(user?.IdUser ?? 0, isDoctor:false, isHistoric:false);
                if (!scheduleResp.WasSuccessful)
                    sb.AppendLine($"Falha ao carregar os dados das consultas agendadas. \nErro:{scheduleResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                var userSchedule = scheduleResp.Response;
                nextAppointment = userSchedule?
                    .Where(s => s.Status == (int)ScheduleStatus.Confirmed)
                    .OrderBy(s => s.AppointmentDate)
                    .FirstOrDefault();

                if (nextAppointment is null)
                {
                    nextAppointment = userSchedule?
                        .Where(s => s.Status == (int)ScheduleStatus.Pending)
                        .OrderBy(s => s.AppointmentDate)
                        .FirstOrDefault();

                    await MainThread.InvokeOnMainThreadAsync(() => NextAppointmentIsPending = true);
                }

                var appntResp = await appointmentService.GetAllAsync();
                if (!appntResp.WasSuccessful)
                    sb.AppendLine($"Falha ao carregar os dados dos serviços de atendimento disponíveis.\nErro:{appntResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                var surgeries = appntResp?.Response?
                    .Where(s => s.Idappointmenttype == (int)Models.AppointmentType.Surgery)
                    .OrderByDescending(s => s.Ispopular)
                    .Take(4)
                    .ToList() ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AvailableSurgeries.Clear();
                    surgeries.ForEach(s => AvailableSurgeries.Add(s));
                    CanShowAvailableSurgeries = surgeries.Count > 0;
                });

                var exams = appntResp?.Response?
                    .Where(e => e.Idappointmenttype == (int)Models.AppointmentType.Examination)
                    .OrderByDescending(e => e.Ispopular)
                    .Take(4)
                    .ToList() ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AvailableExams.Clear();
                    exams.ForEach(e => AvailableExams.Add(e));
                    CanShowAvailableExams = exams.Count > 0;
                });

                if (nextAppointment is null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => UserHasNextAppointment = false);
                    return null;
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    UserHasNextAppointment = true;
                    NextAppointmentTitle = nextAppointment?.AppointmentTitle ?? string.Empty;
                    NextAppointmentNote = nextAppointment?.Obs ?? string.Empty;
                    NextAppointmentTypePrefix = "Sua";
                    NextAppointmentTypeSufix = "a";
                    NextAppointmentDoctorName = nextAppointment?.ProfessionalDoctorName ?? "Dr...";
                    NextAppointmentDoctorMainSpecialty = nextAppointment?.DoctorSpecialty ?? string.Empty;
                    NextAppointmentIsOnline = nextAppointment?.IsOnline == (sbyte)1;
                    NextAppointmentStatus = EnumTranslator.TranslateScheduleStatus(nextAppointment?.Status ?? 0,
                        nextAppointment != null ? (Models.AppointmentType)nextAppointment.AppointmentType : Models.AppointmentType.Consultation);
                    NextAppointmentStatusID = nextAppointment?.Status ?? 0;
                    NextAppointmentDate = nextAppointment?.AppointmentDate?.ToString("dddd, dd 'de' MMMM 'de' yyyy", Master.Culture) ?? string.Empty;
                    NextAppointmentHour = TimeOnly.FromDateTime(nextAppointment?.AppointmentDate ?? DateTime.Now).ToString("HH:mm");
                    NextAppointmentWasRescheduled = nextAppointment?.Rescheduled == (sbyte)1;
                    NextAppointmentOriginalDate = nextAppointment?.OriginalDate?.ToString("dddd, dd 'de' MMMM 'de' yyyy", Master.Culture) ?? string.Empty;
                    NextAppointmentPendingPayment = nextAppointment?.PendingPayment == (sbyte)1 && nextAppointment?.Status == (int)ScheduleStatus.Confirmed;

                    switch (nextAppointment?.AppointmentType)
                    {
                        case (int)Models.AppointmentType.Consultation:
                            NextAppointmentType = "Consulta";
                            break;
                        case (int)Models.AppointmentType.Examination:
                            NextAppointmentType = "Exame";
                            NextAppointmentTypePrefix = "Seu";
                            NextAppointmentTypeSufix = "o";
                            break;
                        case (int)Models.AppointmentType.Surgery:
                            NextAppointmentType = "Cirurgia";
                            break;
                        default:
                            NextAppointmentType = "Consulta";
                            break;
                    }
                });

                var clinicResp = await clinicService.GetByIdAsync(1);
                if (!clinicResp.WasSuccessful)
                    sb.AppendLine($"Não foi possível carregar os dados da clínica.\nErro:{clinicResp.ResponseErr}");

                Master.GlobalToken.ThrowIfCancellationRequested();

                clinic = clinicResp.Response ??
                    throw new Exception("Falha ao carregar os dados da clínica. Contate um administrador para mais detalhes");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ClinicName = clinic.Fantasyname ?? string.Empty;
                    ClinicComplement = clinic.Complement ?? string.Empty;
                    ClinicFullAdress = string.Concat(clinic.Adress, ", ", clinic.Number, " - ", clinic.Neighborhood, ", ", clinic.City, " - ", clinic.State);

                    if (!string.IsNullOrEmpty(ClinicComplement.Trim()))
                        ClinicComplement = $" - {ClinicComplement}";

                    if (NextAppointmentIsOnline)
                    {
                        ClinicIcon = "\ue821";
                        ClinicName = "Consulta Online";
                        ClinicComplement = string.Empty;
                        ClinicFullAdress = string.Empty;
                        HowToGetThereIcon = ClinicIcon;
                        HowToGetThereText = "Entrar Online";
                    }
                });

                if (!string.IsNullOrWhiteSpace(sb.ToString().Trim()))
                {
                    await MainThread.InvokeOnMainThreadAsync(() => UserHasNextAppointment = false);
                    throw new Exception(sb.ToString());
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
                IsLoading = false;
            }
        }

        [RelayCommand] private void ShowPremiumPlans() => WeakReferenceMessenger.Default.Send(new ShowProfilePageSelectTabMessage("2"));
        [RelayCommand] private void SwitchToSchedulePageSelectAppointment(Appointment appointment)
           => WeakReferenceMessenger.Default.Send(new ShowSchedulePageSelectAppointmentMessage(appointment.Idappointment));
        [RelayCommand] private void OpenMenu() => Navigator.OpenFlyoutPage();
        [RelayCommand] private void SwitchToPaymentPage() => Navigator.SwitchPatientPage(PatientPage.Payment);
        [RelayCommand] private void SwitchToSchedulePage() => Navigator.SwitchPatientPage(PatientPage.Schedule);
        [RelayCommand] private async Task OnPatientCall() => await PhoneHelper.CallAsync(clinic?.Fone);
        [RelayCommand]
        private async Task HowGetThereOrPushOnline()
        {
            if (NextAppointmentIsOnline && nextAppointment is not null)
                await Navigator.PushOnlinePage(nextAppointment);
            else
                await MapsHelper.OpenRouteByCepAsync(cep: clinic?.Postalcode, number: clinic?.Number);
        }

        [RelayCommand] private async Task OnReschedule()
        {
            var returnValue = await PopupHelper<ScheduleDto>.PushInstanceAsync<ReschedulePopup>(nextAppointment);

            if (!returnValue)
                return;

            await Messenger.ShowMessage("Consulta reagendada\n\nFique atento ao calendário emm 😉!", "Sucesso");
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(PatientPage.Home.ToString()));
        }

        [RelayCommand] private async Task OnCancel()
        {
            ValueBunker<bool>.SavedValue = false;
            var returnValue = await PopupHelper<ScheduleDto>.PushInstanceAsync<CancelSchedulePopup>(nextAppointment);
            if (!returnValue)
                return;

            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                await PopupHelper.PushLoadingAsync();

                int? oldstatus = nextAppointment.Status;
                nextAppointment.Status = (int)ScheduleStatus.Canceled;
                nextAppointment.PendingPayment = 0;
                nextAppointment.MedicalReport = null;
                nextAppointment.FeedBack = PopupHelper<string>.GetValue();
                nextAppointment.Rating = null;

                var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(nextAppointment, (int)UserType.Patient, string.Empty, oldstatus));
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(resp.ResponseErr);
                }

                await Loader.RunWithLoadingAsync(LoadAllAsync);
                WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(PatientPage.Home.ToString()));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }
        [RelayCommand] private async Task ShowDMCareApp() => await Messenger.ShowMessage("O DM será lançado em breve, assim que tivermos atualizações avisaremos","Aplicativo ainda não lançado");
        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }
    }
}
