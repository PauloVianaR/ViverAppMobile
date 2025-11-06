using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;
using AppointmentType = ViverAppMobile.Models.AppointmentType;

namespace ViverAppMobile.ViewModels.Admin
{
    public partial class AdminClinicViewModel : ObservableObject, IViewModelInstancer
    {
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private string selectedAppointmentType = string.Empty;
        [ObservableProperty] private string selectedAppointmentTypeFilter = string.Empty;
        [ObservableProperty] private string cnpj = string.Empty;
        [ObservableProperty] private string corporateReason = string.Empty;
        [ObservableProperty] private string fantasyName = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string adress = string.Empty;
        [ObservableProperty] private string number = string.Empty;
        [ObservableProperty] private string neighborhood = string.Empty;
        [ObservableProperty] private string city = string.Empty;
        [ObservableProperty] private string state = string.Empty;
        [ObservableProperty] private string cep = string.Empty;
        [ObservableProperty] private string complement = string.Empty;
        [ObservableProperty] private string selectedTab = "0";
        [ObservableProperty] private string appointmentTitle = string.Empty;
        [ObservableProperty] private string appointmentDescription = string.Empty;
        [ObservableProperty] private string appointmentPrice = string.Empty;
        [ObservableProperty] private string appointmentAverageTimeMin = string.Empty;
        [ObservableProperty] private string appointmentDescritionFilter = string.Empty;
        [ObservableProperty] private string holidayName = string.Empty;
        [ObservableProperty] private DateTime holidayDate = DateTime.Today;

        public DateTime FirstDayOfYear => new (DateTime.Today.Year, 1, 1);
        public DateTime LastDayOfYear => new(DateTime.Today.Year, 12, 31);

        private readonly ClinicService clinicService;
        private readonly AvailabilityClinicService availabilityClinicService;
        private readonly ConfigService configService;
        private readonly AppointmentService appointmentService;
        private readonly OpenCepService openCepService;
        private readonly HolidayService holidayService;
        private List<Appointment> allAppointments = [];
        private List<AvailabilityClinic> allAvailabilitiesClinic = [];
        private List<Config> allConfigs = [];
        private Clinic? clinic;

        public ObservableCollection<AsyncModel<Appointment>> Appointments { get; set; } = [];
        public ObservableCollection<AsyncModel<AvailabilityClinic>> AvailabilitiesClinic { get; set; } = [];
        public ObservableCollection<AsyncModel<Config>> Configs { get; set; } = [];
        public ObservableCollection<Holiday> Holidays { get; set; } = [];        
        public ObservableCollection<string> AppointmentTypes { get; set; } = [];
        public ObservableCollection<string> AppointmentTypesFilter { get; set; } = [];

        #region All
        [RelayCommand] private void SwitchSelectedTab(string newTab) => SelectedTab = newTab;

        public AdminClinicViewModel()
        {
            clinicService = new();
            availabilityClinicService = new();
            configService = new();
            appointmentService = new();
            openCepService = new();
            holidayService = new();

            AppointmentTypes = ["Selecione o tipo", "\ue811  Consulta", "\ue817  Exame", "\ue806  Cirurgia"];
            AppointmentTypesFilter = ["Todos os tipos", "\ue811  Consulta", "\ue817  Exame", "\ue806  Cirurgia"];
            SelectedAppointmentType = AppointmentTypes[0];
            SelectedAppointmentTypeFilter = AppointmentTypesFilter[0];
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
                Master.GlobalToken.ThrowIfCancellationRequested();

                var clinicResp = await clinicService.GetByIdAsync(1);
                if (!clinicResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    clinicResp.ThrowIfIsNotSucess();
                }

                clinic = clinicResp?.Response;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    CorporateReason = clinic?.Corporatereason ?? string.Empty;
                    FantasyName = clinic?.Fantasyname ?? string.Empty;
                    Cnpj = clinic?.Cnpj ?? string.Empty;
                    Phone = clinic?.Fone ?? string.Empty;
                    Email = clinic?.Email ?? string.Empty;
                    Adress = clinic?.Adress ?? string.Empty;
                    Number = clinic?.Number ?? string.Empty;
                    Neighborhood = clinic?.Neighborhood ?? string.Empty;
                    Complement = clinic?.Complement ?? string.Empty;
                    City = clinic?.City ?? string.Empty;
                    State = clinic?.State ?? string.Empty;
                    Cep = clinic?.Postalcode ?? string.Empty;
                });

                var appntResp = await appointmentService.GetAllAsync();
                if (!appntResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    appntResp.ThrowIfIsNotSucess();
                }

                allAppointments = appntResp?.Response?
                    .OrderBy(a => a.Idappointmenttype)
                    .ThenBy(a => a.Description)
                    .ToList() ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    allAppointments.ForEach(a =>
                    {
                        AsyncModel<Appointment> model = new(a)
                        {
                            IsActive = a.Status == 1
                        };
                        Appointments.Add(model);
                    });
                });

                var avClinicResp = await availabilityClinicService.GetAllAsync();
                if (!avClinicResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    avClinicResp.ThrowIfIsNotSucess();
                }

                allAvailabilitiesClinic = avClinicResp?.Response?
                    .OrderBy(a => a.Daytype)
                    .ToList() ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    for (int i = (int)DayOfWeek.Sunday; i <= (int)DayOfWeek.Saturday; i++)
                    {
                        var existingAC = allAvailabilitiesClinic.FirstOrDefault(ac => ac.Daytype == i);
                        if(existingAC is null)
                        {
                            var model = new AsyncModel<AvailabilityClinic>(new AvailabilityClinic
                            {
                                Idavailabilityclinic = 0,
                                Idclinic = 1,
                                Daytype = i,
                                Starttime = new TimeOnly(8, 0),
                                Endtime = new TimeOnly(17, 0),
                            })
                            {
                                IsActive = false
                            };

                            AvailabilitiesClinic.Add(model);
                            continue;
                        }

                        var existingmodel = new AsyncModel<AvailabilityClinic>(existingAC)
                        {
                            IsActive = true
                        };

                        AvailabilitiesClinic.Add(existingmodel);
                    }
                });

                var configResp = await configService.GetAllAsync();
                if (!configResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    configResp.ThrowIfIsNotSucess();
                }

                allConfigs = configResp?.Response?.ToList() ?? [];
                allConfigs.ForEach(c => Configs.Add(new AsyncModel<Config>(c)
                {
                    IsActive = c.Valueisbool == (sbyte)1 && c.Value == 1
                }));

                var holidayResp = await holidayService.GetAllAsync();
                if (!holidayResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    holidayResp.ThrowIfIsNotSucess();
                }

                holidayResp?.Response?
                    .OrderBy(h => h.Holidaydate)
                    .ToList()
                    .ForEach(h => Holidays.Add(h));

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

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;

            SelectedAppointmentType = string.Empty;
            SelectedAppointmentTypeFilter = string.Empty;
            Cnpj = string.Empty;
            CorporateReason = string.Empty;
            FantasyName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Adress = string.Empty;
            Number = string.Empty;
            Neighborhood = string.Empty;
            City = string.Empty;
            State = string.Empty;
            Cep = string.Empty;
            Complement = string.Empty;
            AppointmentDescription = string.Empty;
            AppointmentPrice = string.Empty;
            AppointmentAverageTimeMin = string.Empty;
            AppointmentDescritionFilter = string.Empty;
            SelectedAppointmentType = AppointmentTypes[0];
            SelectedAppointmentTypeFilter = AppointmentTypesFilter[0];
            Appointments.Clear();
            AvailabilitiesClinic.Clear();
            HolidayName = string.Empty;
            HolidayDate = DateTime.Today;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        #endregion

        #region Information

        partial void OnCepChanged(string value)
        {
            if (Cep.Length == 9)
                _ = SearchAdressByCep();
        }

        [RelayCommand]
        private async Task SearchAdressByCep()
        {
            if (string.IsNullOrWhiteSpace(Cep))
                return;

            try
            {
                await PopupHelper.PushLoadingAsync();
                var resp = await openCepService.FindAdressByCepAsync(Cep);
                if (!resp.WasSuccessful)
                    resp.ThrowIfIsNotSucess();

                var findedAdress = resp.Response;

                Adress = findedAdress.Logradouro ?? string.Empty;
                Neighborhood = findedAdress.Bairro ?? string.Empty;
                City = findedAdress.Localidade ?? string.Empty;
                State = findedAdress.Uf ?? string.Empty;
            }
            catch (Exception ex)
            {
                await PopupHelper.PopAllPopUpAsync();
                Debug.WriteLine(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();

        }

        [RelayCommand] private async Task SaveClinicInfo()
        {
            try
            {
                await PopupHelper.PushLoadingAsync();

                if (!ValidationHelper.IsEmailValid(Email))
                    throw new Exception("Email inválido");
                if (!ValidationHelper.IsCnpjValid(Cnpj))
                    throw new Exception("Cnpj inválido");
                if (!ValidationHelper.IsPhoneValid(Phone))
                    throw new Exception("Telefone inválido");
                if (!ValidationHelper.IsValidCep(Cep))
                    throw new Exception("CEP inválido");
                if (string.IsNullOrWhiteSpace(CorporateReason))
                    throw new Exception("Informe a Razão Social");
                if (string.IsNullOrWhiteSpace(FantasyName))
                    throw new Exception("Informe o Nome Fantasia");
                if (string.IsNullOrWhiteSpace(Adress))
                    throw new Exception("Informe o logradouro");
                if (string.IsNullOrWhiteSpace(Neighborhood))
                    throw new Exception("Informe o bairro");
                if (string.IsNullOrWhiteSpace(City))
                    throw new Exception("Informe a cidade");
                if (string.IsNullOrWhiteSpace(State))
                    throw new Exception("Informe o estado");

                clinic.Cnpj = Cnpj;
                clinic.Email = Email;
                clinic.Adress = Adress;
                clinic.Corporatereason = CorporateReason;
                clinic.Fantasyname = FantasyName;
                clinic.Complement = Complement;
                clinic.Number = Number;
                clinic.Postalcode = Cep;
                clinic.Fone = Phone;
                clinic.City = City;
                clinic.State = State;

                var resp = await clinicService.UpdateAsync(clinic.Idclinic, clinic);
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                await Messenger.ShowToastMessage("Alterações concluídas");
            }
            catch (OperationCanceledException) { }
            catch(Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        #endregion

        #region Services
        [RelayCommand] private async Task AddAppointment()
        {
            try
            {
                if (SelectedAppointmentType is null)
                    throw new Exception("Selecione o tipo base de serviço");
                if (SelectedAppointmentType == AppointmentTypes[0] || AppointmentTypes.IndexOf(SelectedAppointmentType) == -1)
                    throw new Exception("Selecione o tipo base de serviço");
                if (string.IsNullOrWhiteSpace(AppointmentTitle))
                    throw new Exception("Informe o nome do serviço");
                if (string.IsNullOrWhiteSpace(AppointmentDescription))
                    throw new Exception("Informe a descrição do serviço");
                if (string.IsNullOrWhiteSpace(AppointmentPrice))
                    throw new Exception("Informe o valor do serviço");
                if (string.IsNullOrWhiteSpace(AppointmentAverageTimeMin))
                    throw new Exception("Informe o tempo médio de duração do serviço");
                if (!decimal.TryParse(AppointmentPrice, out decimal price))
                    throw new Exception("Valor inválido para o preço do serviço");
                if (!int.TryParse(AppointmentAverageTimeMin, out int appntMins))
                    throw new Exception("Valor inválido para o tempo médio do serviço");

                var appntType = (AppointmentType)AppointmentTypes.IndexOf(SelectedAppointmentType);

                await PopupHelper.PushLoadingAsync();
                var resp = await appointmentService.CreateAsync(new Appointment
                {
                    Idappointmenttype = (int)appntType,
                    Title = AppointmentTitle,
                    Description = AppointmentDescription,
                    Averagetime = new TimeOnly(0,0).AddMinutes(appntMins),
                    Price = price,
                    Ispopular = 0,
                    Canonline = appntType == AppointmentType.Consultation ? (sbyte)1 : (sbyte)0,
                    Status = 1,
                });
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                var newAppnt = resp?.Response;
                if (newAppnt is null)
                    return;

                allAppointments.Add(newAppnt);
                allAppointments = allAppointments
                    .OrderBy(a => a.Idappointmenttype)
                    .ThenBy(a => a.Description)
                    .ToList();

                Appointments.Clear();
                allAppointments.ForEach(a => Appointments.Add(new AsyncModel<Appointment>(a)
                {
                    IsActive = a.Status == 1
                }));

                SelectedAppointmentType = AppointmentTypes[0];
                AppointmentTitle = string.Empty;
                AppointmentDescription = string.Empty;
                AppointmentPrice = string.Empty;
                AppointmentAverageTimeMin = string.Empty;

                await Messenger.ShowToastMessage("Serviço Inserido");
            }
            catch(OperationCanceledException) { }
            catch(Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand]
        private void FilterAppointments()
        {
            Appointments.Clear();

            var query = allAppointments.ToList();

            if (!string.IsNullOrWhiteSpace(AppointmentDescritionFilter))
                query = [.. query.Where(a => a.Description.Contains(AppointmentDescritionFilter.Trim(), StringComparison.OrdinalIgnoreCase))];

            int index = AppointmentTypesFilter.IndexOf(SelectedAppointmentTypeFilter);

            if (index >= 1)
                query = [.. query.Where(a => a.Idappointmenttype == index)];

            query.ForEach(a => Appointments.Add(new AsyncModel<Appointment>(a)));
        }

        partial void OnSelectedAppointmentTypeFilterChanged(string value) => FilterAppointments();

        [RelayCommand] private async Task ToggleAppointmentStatusAsync(AsyncModel<Appointment> appnt)
        {
            await appnt.ExecuteAsync(async model =>
            {
                try
                {
                    int oldstatus = model.Status == 1 ? 0 : 1;

                    var resp = await appointmentService.UpdateAsync(model.Idappointment, model);
                    if (!resp.WasSuccessful)
                    {
                        model.Status = oldstatus;
                        appnt.IsActive = oldstatus == 1;
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }
                }
                catch (OperationCanceledException) { }
                catch(Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand] private async Task RemoveAppointment(AsyncModel<Appointment> appnt)
        {
            await appnt.ExecuteAsync(async model =>
            {
                try
                {
                    var resp = await appointmentService.ExistsInSchedule(model.Idappointment);
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }

                    bool exists = resp.Response;
                    if (exists)
                        throw new Exception("Parece que já existe um agendamento vinculado a este serviço...\nVocê não pode excluí-lo, mas pode inativá-lo para evitar futuros agendamentos com ele.");

                    if (!await Messenger.ShowQuestionMessage("Gostaria mesmo de remover este serviço?\n\nObs: Esta ação não pode ser desfeita.", "Confirmação"))
                        return;

                    var deleteResp = await appointmentService.DeleteAsync(model.Idappointment);
                    if (!deleteResp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        deleteResp.ThrowIfIsNotSucess();
                    }

                    var existingappnt = allAppointments.FirstOrDefault(a => a.Idappointment == model.Idappointment);
                    if(existingappnt is not null)
                    {
                        allAppointments.Remove(existingappnt);
                    }
                    
                    Appointments.Remove(appnt);
                    await Messenger.ShowToastMessage("Serviço Removido");
                }
                catch (OperationCanceledException) { }
                catch(Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message,"Ops...");
                }
            });
        }

        [RelayCommand] private async Task EditAppointment(AsyncModel<Appointment> appnt)
        {
            await appnt.ExecuteAsync(async model =>
            {
                var result = await PopupHelper<Appointment>.PushInstanceAsync<EditAppointmentPopup>(model);
                if (!result)
                    return;

                var editedAppnt = PopupHelper<Appointment>.GetValue();
                if (editedAppnt is null)
                    return;

                try
                {
                    var resp = await appointmentService.UpdateAsync(editedAppnt.Idappointment,editedAppnt);
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }

                    allAppointments[allAppointments.IndexOf(model)] = editedAppnt;
                    Appointments[Appointments.IndexOf(appnt)] = new AsyncModel<Appointment>(editedAppnt) { IsActive = editedAppnt.Status == 1 };
                    Appointments = Appointments.OrderBy(s => s.Model.Idappointmenttype).ThenBy(s => s.Model.Idappointment).ToObservableCollection();
                    OnPropertyChanged(nameof(Appointments));

                    await Messenger.ShowToastMessage("Alteração Concluída");
                }
                catch (OperationCanceledException) { }
                catch(Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });

        }

        #endregion

        #region Availability

        [RelayCommand]
        private async Task ToggleAvailiabilityClinicStatusAsync(AsyncModel<AvailabilityClinic> ac)
        {
            await ac.ExecuteAsync(async model =>
            {
                try
                {
                    var existingAc = allAvailabilitiesClinic.FirstOrDefault(a => a.Idavailabilityclinic == model.Idavailabilityclinic);

                    if (ac.IsActive)
                    {
                        if (existingAc is not null)
                            return;

                        var insertResp = await availabilityClinicService.CreateAsync(model);
                        if (!insertResp.WasSuccessful)
                        {
                            ac.IsActive = !ac.IsActive;
                            Master.GlobalToken.ThrowIfCancellationRequested();
                            insertResp.ThrowIfIsNotSucess();
                        }

                        model.Idavailabilityclinic = insertResp?.Response?.Idavailabilityclinic ?? 0;
                        allAvailabilitiesClinic.Add(model);
                        return;
                    }

                    if (existingAc is null)
                        return;

                    var deleteResp = await availabilityClinicService.DeleteAsync(model.Idavailabilityclinic);
                    if (!deleteResp.WasSuccessful)
                    {
                        ac.IsActive = !ac.IsActive;
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        deleteResp.ThrowIfIsNotSucess();
                    }

                    model.Idavailabilityclinic = 0;
                    model.Starttime = new TimeOnly(0, 0);
                    model.Endtime = new TimeOnly(0, 0);
                    allAvailabilitiesClinic.Remove(model);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand]
        private async Task UpdateAvailabilitiesClinicAsync()
        {
            StringBuilder sb = new();

            await PopupHelper.PushLoadingAsync();

            foreach (var ac in AvailabilitiesClinic)
            {
                if (Master.GlobalToken.IsCancellationRequested)
                    break;

                if (ac.Model.Idavailabilityclinic == 0)
                    continue;
                
                await ac.ExecuteAsync(async model =>
                {
                    try
                    {
                        if (!ac.IsActive)
                            return;

                        var resp = await availabilityClinicService.UpdateAsync(model.Idavailabilityclinic, model);
                        if (!resp.WasSuccessful)
                        {
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
                await Messenger.ShowErrorMessageAsync(sb.ToString());
        }

        [RelayCommand] private async Task AddNewHoliday()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(HolidayName))
                    throw new Exception("Informe o nome do feriado a ser adicionado");

                await PopupHelper.PushLoadingAsync();

                var resp = await holidayService.CreateAsync(new Holiday
                {
                    Holidayname = this.HolidayName,
                    Holidaydate = DateOnly.FromDateTime(this.HolidayDate),
                    Canschedule = (sbyte)1
                });
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                var newHoliday = resp.Response;
                if (newHoliday is null)
                    return;

                Holidays.Add(newHoliday);
                this.SortHolidayList();

                HolidayName = string.Empty;
                HolidayDate = DateTime.Today;
            }
            catch (OperationCanceledException) { }
            catch(Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand] private async Task DropHoliday(Holiday holiday)
        {
            if (!await Messenger.ShowQuestionMessage($"Tem certeza que deseja remover o feriado {holiday.Holidayname}?","Remover Feriado"))
                return;

            try
            {
                await PopupHelper.PushLoadingAsync();

                var resp = await holidayService.DeleteAsync(holiday.Idholiday);
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                Holidays.Remove(holiday);
                this.SortHolidayList();
            }
            catch (OperationCanceledException) { }
            catch(Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        private void SortHolidayList()
        {
            var currentHolidays = Holidays.OrderBy(h => h.Holidaydate).ToList(); // shallow copy
            Holidays.Clear();
            currentHolidays.ForEach(h => Holidays.Add(h));
        }

        #endregion

        #region Configs

        [RelayCommand] private async Task ToggleConfigStatus(AsyncModel<Config> config)
        {
            await config.ExecuteAsync(async model =>
            {
                try
                {
                    if (model.Valueisbool != (sbyte)1)
                        return;

                    int oldStatus = model.Value == 1 ? 0 : 1;

                    var resp = await configService.UpdateAsync(model.Idconfig,model);
                    if (!resp.WasSuccessful)
                    {
                        model.Value = oldStatus;
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }
                }
                catch (OperationCanceledException) { }
                catch(Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand] private async Task UpdateNotBoolConfig(AsyncModel<Config> config)
        {
            await config.ExecuteAsync(async model =>
            {
                try
                {
                    if (model.Valueisbool == (sbyte)1)
                        return;

                    var resp = await configService.UpdateAsync(model.Idconfig, model);
                    if (!resp.WasSuccessful)
                    {
                        var currentConfig = allConfigs.FirstOrDefault(c => c.Idconfig == model.Idconfig);
                        if(currentConfig is not null)
                            model.Value = currentConfig.Value;

                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }
                }
                catch(OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        #endregion
    }
}
