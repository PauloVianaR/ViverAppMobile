using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using System.Collections.ObjectModel;
using System.Text;
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
    public partial class DoctorProfileViewModel : ObservableObject, IViewModelInstancer
    {
        private bool isLoading = false;
        private bool isFiltring = false;
        private readonly AuthService authService;
        private readonly UserService userService;
        private readonly DoctorPropsService doctorPropsService;
        private readonly SpecialtyDoctorService specialtysDoctorService;
        private readonly AppointmentService appointmentService;
        private readonly AvailabilityDoctorService availabilityDoctorService;
        private UserDto? user; 
        private DoctorDto? doctorDto;
        private List<AvailabilityDoctor> allAvailabilitiesDoctor = [];

        [ObservableProperty] private int consultationsSelectedCount = 0;
        [ObservableProperty] private int examinationsSelectedCount = 0;
        [ObservableProperty] private int surgeriesSelectedCount = 0;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private string selectedTab = "0";
        [ObservableProperty] private string fullName = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string cpf = string.Empty;
        [ObservableProperty] private string crm = string.Empty;
        [ObservableProperty] private string medicalExperience = string.Empty;
        [ObservableProperty] private string selectedMainSpecialty = string.Empty;
        [ObservableProperty] private string selectedTitle = string.Empty;
        [ObservableProperty] private bool notifyEmail = false;
        [ObservableProperty] private bool notifyPush = false;
        [ObservableProperty] private string consultationFilterString = string.Empty;
        [ObservableProperty] private string examinationFilterString = string.Empty;
        [ObservableProperty] private string surgeryFilterString = string.Empty;
        [ObservableProperty] private ObservableCollection<AsyncModel<Appointment>> consultations = [];
        [ObservableProperty] private ObservableCollection<AsyncModel<Appointment>> examinations = [];
        [ObservableProperty] private ObservableCollection<AsyncModel<Appointment>> surgeries = [];
        [ObservableProperty] AsyncModel<DoctorProp> docprop = null!;
        [ObservableProperty] private AsyncModel<double> maxPresencialDayConsultationValue = new(1, canSetFalseActiveModelChanged: true);
        [ObservableProperty] private AsyncModel<double> maxOnlineDayConsultationValue = new(1, canSetFalseActiveModelChanged: true);

        public List<SpecialtysDoctor> SpecialtiesDoctor { get; set; } = [];
        public List<string> DoctorTitles { get; } = ["Dr.", "Dra."];
        public List<AsyncModel<Appointment>> Allappointments { get; set; } = [];
        public ObservableCollection<string> Specialties { get; } = [];
        public ObservableCollection<AsyncModel<AvailabilityDoctor>> AvailabilitiesDoctorPresencial { get; set; } = [];
        public ObservableCollection<AsyncModel<AvailabilityDoctor>> AvailabilitiesDoctorOnline { get; set; } = [];

        public DoctorProfileViewModel()
        {
            authService = new();
            userService = new();
            doctorPropsService = new();
            specialtysDoctorService = new();
            appointmentService = new();
            availabilityDoctorService = new();

            Specialties =
            [
                "Oftamologia",
                "Oftamologia Pediátrica",
                "Cirurgia Oftamológica",
                "Retina e Vítreo",
                "Glaucoma",
                "Córnea e doenças externas"
            ];
            SelectedMainSpecialty = Specialties[0];
            SelectedTitle = DoctorTitles[0];
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

            try
            {
                if (user is null)
                    return null;

                var userResp = await userService.GetDoctorAsync(user.IdUser);
                if (!userResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    userResp.ThrowIfIsNotSucess();
                }

                doctorDto = userResp.Response;
                if (doctorDto is null)
                    throw new Exception("Falha ao tentar carregar as informações do seu usuário.\nTente fazer o login novamente.");                

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Docprop = new(new(doctorDto)) { IsActive = doctorDto.Attendonline == 1 };
                    FullName = doctorDto.Name ?? string.Empty;
                    Email = doctorDto.Email ?? string.Empty;
                    Phone = doctorDto.Fone ?? string.Empty;
                    Cpf = doctorDto.Cpf ?? string.Empty;
                    Crm = doctorDto.Crm ?? string.Empty;
                    MedicalExperience = doctorDto.Medicalexperience?.ToString() ?? string.Empty;
                    SelectedMainSpecialty = doctorDto.Mainspecialty ?? string.Empty;
                    SelectedTitle = doctorDto.Title ?? string.Empty;
                    MaxOnlineDayConsultationValue.Model = doctorDto.Maxonlinedayconsultation;
                    MaxPresencialDayConsultationValue.Model = doctorDto.Maxpresencialdayconsultation;
                    NotifyEmail = (doctorDto.NotifyEmail ?? 0) == 1;
                    NotifyPush = (doctorDto.Notifypush ?? 0) == 1;

                    MaxPresencialDayConsultationValue.IsActive = true;
                    MaxOnlineDayConsultationValue.IsActive = true;
                });

                var appntResp = await appointmentService.GetAllAsync();
                if (!appntResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    appntResp.ThrowIfIsNotSucess();
                }

                var allappointmentsResp = appntResp.Response;

                if (allappointmentsResp is null)
                    return null;
                if (!allappointmentsResp.Any())
                    return null;

                List<ObservableCollection<AsyncModel<Appointment>>> collections = [Consultations, Examinations, Surgeries];
                foreach (var a in allappointmentsResp)
                {
                    if (a.Status != 1)
                        continue;

                    int index = a.Idappointmenttype - 1;
                    if (index < 0)
                        continue;

                    var newModel = new AsyncModel<Appointment>(a) { IsActive = false };
                    collections[index].Add(newModel);
                    Allappointments.Add(newModel);
                }

                var specialtiesResp = await specialtysDoctorService.GetAllByDoctorAsync(user.IdUser);
                if (!specialtiesResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    specialtiesResp.ThrowIfIsNotSucess();
                }

                SpecialtiesDoctor = specialtiesResp.Response?.ToList() ?? [];
                foreach (var sd in SpecialtiesDoctor)
                {
                    var consultation = Consultations.FirstOrDefault(c => c.Model.Idappointment == sd.Idappointment);
                    if(consultation is not null)
                    {
                        consultation.IsActive = true;
                        ConsultationsSelectedCount++;
                        continue;
                    }

                    var examination = Examinations.FirstOrDefault(e => e.Model.Idappointment == sd.Idappointment);
                    if(examination is not null)
                    {
                        examination.IsActive = true;
                        ExaminationsSelectedCount++;
                        continue;
                    }

                    var surgery = Surgeries.FirstOrDefault(s => s.Model.Idappointment == sd.Idappointment);
                    if(surgery is not null)
                    {
                        surgery.IsActive = true;
                        SurgeriesSelectedCount++;
                    }
                }

                var avDocResp = await availabilityDoctorService.GetAllAsync(user.IdUser);
                if (!avDocResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    avDocResp.ThrowIfIsNotSucess();
                }

                allAvailabilitiesDoctor = avDocResp.Response?
                    .OrderBy(a => a.Daytype)
                    .ToList() ?? [];

                var allAvDocPresencial = allAvailabilitiesDoctor
                    .Where(ad => ad.Isonline == 0)
                    .ToList() ?? [];

                var allAvDocOnline = allAvailabilitiesDoctor
                    .Where(ad => ad.Isonline == 1)
                    .ToList() ?? [];

                void PopulateAvCollection(List<AvailabilityDoctor> listbase, ObservableCollection<AsyncModel<AvailabilityDoctor>> collection, sbyte isonline)
                {
                    for (int i = (int)DayOfWeek.Sunday; i <= (int)DayOfWeek.Saturday; i++)
                    {
                        var existingAD = listbase.FirstOrDefault(ac => ac.Daytype == i);
                        if (existingAD is null)
                        {
                            AsyncModel<AvailabilityDoctor> model = new(new AvailabilityDoctor
                            {
                                Idavailabilitydoctor = 0,
                                Iddoctor = user.IdUser,
                                Daytype = i,
                                Starttime = new TimeOnly(8, 0),
                                Endtime = new TimeOnly(17, 0),
                                Isonline = isonline
                            })
                            {
                                IsActive = false
                            };

                            collection.Add(model);
                            continue;
                        }

                        collection.Add(new AsyncModel<AvailabilityDoctor>(existingAD)
                        {
                            IsActive = true
                        });
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PopulateAvCollection(allAvDocPresencial, AvailabilitiesDoctorPresencial, 0);
                    PopulateAvCollection(allAvDocOnline, AvailabilitiesDoctorOnline, 1);
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

        [RelayCommand] private void SelectTab(string newTab) => SelectedTab = newTab;

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        [RelayCommand] private async Task SaveDoctorChanges()
        {
            if (user is null || doctorDto is null)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(FullName))
                    throw new Exception("O nome informado não pode ser vazio");
                if (!ValidationHelper.IsEmailValid(Email))
                    throw new Exception("O novo email informado é inválido");
                if (!ValidationHelper.IsCPFValid(Cpf))
                    throw new Exception("O CPF informado é inválido");
                if (!ValidationHelper.IsPhoneValid(Phone))
                    throw new Exception("O novo telefone informado é inválido");
                if (!ValidationHelper.IsCRMValid(Crm))
                    throw new Exception("O CRM informado é inválido");
                if (!ValidationHelper.IsNumber(MedicalExperience))
                    throw new Exception("O tempo de experiência médica informado é inválido");
                if (string.IsNullOrWhiteSpace(SelectedTitle))
                    throw new Exception("Por favor, informe seu título profissional (Dr ou Dra)");
                if (string.IsNullOrWhiteSpace(SelectedMainSpecialty))
                    throw new Exception("Por favor, indique sua principal especialidade");

                await PopupHelper.PushLoadingAsync();

                user.Name = FullName;
                user.Email = Email;
                user.Cpf = Cpf;
                user.Fone = Phone;
                user.NotifyEmail = NotifyEmail ? (sbyte)1 : (sbyte)0;
                user.Notifypush = NotifyPush ? (sbyte)1 : (sbyte)0;

                var userResp = await userService.UpdateAsync(user.IdUser, user);
                if (!userResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    userResp.ThrowIfIsNotSucess();
                }

                var dp = docprop.Model ?? throw new Exception("Falha ao atualizar seus dados profissionais");
                dp.Crm = Crm;
                dp.Title = SelectedTitle;
                dp.Mainspecialty = SelectedMainSpecialty;
                dp.Medicalexperience = int.TryParse(MedicalExperience, out int medexp) ? medexp : 0;

                var docPropResp = await doctorPropsService.UpdateAsync(dp.Iddoctorprops, dp);
                if (!docPropResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    docPropResp.ThrowIfIsNotSucess();
                }

                await Messenger.ShowToastMessage("Dados Salvos!");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }
            finally
            {
                await PopupHelper.PopAllPopUpAsync();
            }
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            if (user is null)
                return;

            var returnValue = await PopupHelper<object>.PushInstanceAsync<ChangePasswordPopup>(new object());

            if (!returnValue)
                return;

            var passwords = PopupHelper<string[]>.GetValue();

            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                string oldpass = passwords[0];
                string newpass = passwords[1];

                await PopupHelper.PushLoadingAsync();

                var resp = await authService.ChangeUserPassword(user, oldpass, newpass);
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(resp.ResponseErr);
                }

                await Messenger.ShowToastMessage("Alteração concluída. Talvez seja necessário refazer o login");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand]
        private async Task SetSpeciltyDoctor(AsyncModel<Appointment> appnt)
        {
            await appnt.ExecuteAsync(async model =>
            {
                int appntType = model.Idappointmenttype - 1;

                try
                {
                    if (appnt.IsActive)
                    {
                        var spInsertResp = await specialtysDoctorService.CreateAsync(new SpecialtysDoctor(iddoctor:user.IdUser, idappnt: model.Idappointment));
                        if (!spInsertResp.WasSuccessful)
                        {
                            Master.GlobalToken.ThrowIfCancellationRequested();
                            spInsertResp.ThrowIfIsNotSucess();
                        }

                        var newsp = spInsertResp.Response;
                        if (newsp is null)
                            return;

                        SpecialtiesDoctor.Add(newsp);

                        switch ((Models.AppointmentType)model.Idappointmenttype)
                        {
                            case Models.AppointmentType.Consultation:
                                ConsultationsSelectedCount++;
                                break;
                            case Models.AppointmentType.Examination:
                                ExaminationsSelectedCount++;
                                break;
                            case Models.AppointmentType.Surgery:
                                SurgeriesSelectedCount++;
                                break;
                        }

                        return;
                    }

                    var sp = SpecialtiesDoctor.FirstOrDefault(sp => sp.Iddoctor == user.IdUser && sp.Idappointment == model.Idappointment);
                    if (sp is null)
                        return;

                    var spDelResp = await specialtysDoctorService.DeleteAsync(sp.Idspecialtysdoctor);
                    if (!spDelResp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        spDelResp.ThrowIfIsNotSucess();
                    }

                    SpecialtiesDoctor.Remove(sp);

                    switch ((Models.AppointmentType)model.Idappointmenttype)
                    {
                        case Models.AppointmentType.Consultation:
                            ConsultationsSelectedCount--;
                            break;
                        case Models.AppointmentType.Examination:
                            ExaminationsSelectedCount--;
                            break;
                        case Models.AppointmentType.Surgery:
                            SurgeriesSelectedCount--;
                            break;
                    }
                }
                catch(Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        partial void OnConsultationFilterStringChanged(string value)
        {
            this.FilterCollection(Consultations, Models.AppointmentType.Consultation, value);
        }

        partial void OnExaminationFilterStringChanged(string value)
        {
            this.FilterCollection(Examinations, Models.AppointmentType.Examination, value);
        }

        partial void OnSurgeryFilterStringChanged(string value)
        {
            this.FilterCollection(Surgeries, Models.AppointmentType.Surgery, value);
        }

        private void FilterCollection(ObservableCollection<AsyncModel<Appointment>> collection, Models.AppointmentType appntType, string filterString)
        {
            if (isFiltring)
                return;
            isFiltring = true;

            string normalized = filterString.ToLower().Trim();

            collection.Clear();
            var filteredCollection = Allappointments.Where(a => a.Model.Idappointmenttype == (int)appntType);

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                filteredCollection = filteredCollection
                    .Where(a => a.Model.Title.ToLower().Contains(normalized, StringComparison.InvariantCultureIgnoreCase));
            }

            foreach (var item in filteredCollection)
            {
                collection.Add(item);
            }

            isFiltring = false;
        }

        [RelayCommand]
        private async Task ToggleAttendOnline(sbyte attendonline)
        {
            await Docprop.ExecuteAsync(async model =>
            {
                try
                {
                    model.Attendonline = attendonline;
                    var resp = await doctorPropsService.UpdateAsync(model.Iddoctorprops, model);
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand]
        private async Task SaveMaxDaysValue(AsyncModel<double> value)
        {
            if (Docprop is null)
                return;

            bool isonline = ReferenceEquals(value, MaxOnlineDayConsultationValue);
            var model = isonline ? MaxOnlineDayConsultationValue : MaxPresencialDayConsultationValue;
            
            await model.ExecuteAsync(async value =>
            {
                try
                {
                    var dp = Docprop.Model;

                    if (isonline)
                        dp.Maxonlinedayconsultation = (int)MaxOnlineDayConsultationValue.Model;
                    else
                        dp.Maxpresencialdayconsultation = (int)MaxPresencialDayConsultationValue.Model;

                    var resp = await doctorPropsService.UpdateAsync(dp.Iddoctorprops, dp);
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }

                    if (isonline)
                        MaxOnlineDayConsultationValue.IsActive = true;
                    else
                        MaxPresencialDayConsultationValue.IsActive = true;

                    await Messenger.ShowToastMessage("Disponibilidade Salva");
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand]
        private async Task ToggleAvailiabilityClinicStatusAsync(AsyncModel<AvailabilityDoctor> ad)
        {
            await ad.ExecuteAsync(async model =>
            {
                try
                {
                    var existingAd = allAvailabilitiesDoctor.FirstOrDefault(a => a.Idavailabilitydoctor == model.Idavailabilitydoctor);

                    if (ad.IsActive)
                    {
                        if (existingAd is not null)
                            return;

                        var insertResp = await availabilityDoctorService.CreateAsync(model);
                        if (!insertResp.WasSuccessful)
                        {
                            ad.IsActive = !ad.IsActive;
                            Master.GlobalToken.ThrowIfCancellationRequested();
                            insertResp.ThrowIfIsNotSucess();
                        }

                        model.Idavailabilitydoctor = insertResp?.Response?.Idavailabilitydoctor ?? 0;
                        allAvailabilitiesDoctor.Add(model);
                        return;
                    }

                    if (existingAd is null)
                        return;

                    var deleteResp = await availabilityDoctorService.DeleteAsync(model.Idavailabilitydoctor);
                    if (!deleteResp.WasSuccessful)
                    {
                        ad.IsActive = !ad.IsActive;
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        deleteResp.ThrowIfIsNotSucess();
                    }

                    model.Idavailabilitydoctor = 0;
                    model.Starttime = new TimeOnly(0, 0);
                    model.Endtime = new TimeOnly(0, 0);
                    allAvailabilitiesDoctor.Remove(model);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand]
        private async Task UpdateAvailabilitiesDoctorPresencialAsync() => await this.UpdateAvailabilitiesDoctorAsync(isonline: false);
        [RelayCommand]
        private async Task UpdateAvailabilitiesDoctorOnlineAsync() => await this.UpdateAvailabilitiesDoctorAsync(isonline: true);

        private async Task UpdateAvailabilitiesDoctorAsync(bool isonline)
        {
            StringBuilder sb = new();
            await PopupHelper.PushLoadingAsync();

            var adcollection = isonline ? AvailabilitiesDoctorOnline : AvailabilitiesDoctorPresencial;

            foreach (var ad in adcollection)
            {
                if (Master.GlobalToken.IsCancellationRequested)
                    break;

                if (ad.Model.Idavailabilitydoctor == 0)
                    continue;

                await ad.ExecuteAsync(async model =>
                {
                    try
                    {
                        if (!ad.IsActive)
                            return;

                        var resp = await availabilityDoctorService.UpdateAsync(model.Idavailabilitydoctor, model);
                        if (!resp.WasSuccessful)
                        {
                            Master.GlobalToken.ThrowIfCancellationRequested();
                            resp.ThrowIfIsNotSucess();
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        sb.AppendLine(ex.Message);
                    }
                });
            }

            await PopupHelper.PopAllPopUpAsync();

            if (!string.IsNullOrWhiteSpace(sb.ToString().Trim()))
            {
                await Messenger.ShowErrorMessageAsync(sb.ToString());
                return;
            }

            await Messenger.ShowToastMessage("Disponibilidade Salva");
        }
    }
}
