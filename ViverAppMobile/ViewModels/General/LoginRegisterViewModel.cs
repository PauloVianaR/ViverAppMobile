using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Helpers;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Admin;
using ViverAppMobile.Views.Doctor;
using ViverAppMobile.Views.Manager;
using ViverAppMobile.Views.Patient;
using ViverAppMobile.Workers;

#if ANDROID
using ViverAppMobile.Platforms.Android.Services;
#endif

namespace ViverAppMobile.ViewModels.General
{
    public partial class LoginRegisterViewModel : ObservableObject
    {
        private readonly OpenCepService openCepService;
        private readonly AuthService authService;
        private CancellationTokenSource? _timerCts;
        private TimeSpan _remainingTime;
        private bool _isRunningTimer;
        private UserDto? registredUser;
        private bool emailSended = false;

        [ObservableProperty] private int selectedTab = 0;
        [ObservableProperty] private int selectedUserType;
        [ObservableProperty] private bool isLoading = true;
        [ObservableProperty] private bool ispasswordHidden;
        [ObservableProperty] private bool canShowTabs = true;
        [ObservableProperty] private string medicalExperience = string.Empty;
        [ObservableProperty] private string selectedDoctorTitle = string.Empty;
        [ObservableProperty] private string selectedSpecialty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string fullname = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string confirmpassword = string.Empty;
        [ObservableProperty] private string crm = string.Empty;
        [ObservableProperty] private string cpf = string.Empty;
        [ObservableProperty] private string cep = string.Empty;
        [ObservableProperty] private string adress = string.Empty;
        [ObservableProperty] private string neighborhood = string.Empty;
        [ObservableProperty] private string number = string.Empty;
        [ObservableProperty] private string complement = string.Empty;
        [ObservableProperty] private string city = string.Empty;
        [ObservableProperty] private string state = string.Empty;
        [ObservableProperty] private string validationDigit1 = string.Empty;
        [ObservableProperty] private string validationDigit2 = string.Empty;
        [ObservableProperty] private string validationDigit3 = string.Empty;
        [ObservableProperty] private string validationDigit4 = string.Empty;
        [ObservableProperty] private string validationTimer = "15:00";
        [ObservableProperty] private DateTime birthdate = new(2000, 1, 1);

        public DateTime Today => DateTime.Today;
        public DateTime MinimumDate => new(1900, 1, 1);

        public ObservableCollection<string> Specialties { get; } = [];
        public List<string> DoctorTitles { get; } = ["Dr.","Dra."];

        public LoginRegisterViewModel()
        {
            Master.ResetGlobalToken();

            authService = new();
            openCepService = new();
            IspasswordHidden = true;
            SelectedUserType = (int)UserType.Patient;

            Specialties =
            [
                "Selecione a especialidade",
                "Oftamologia",
                "Oftamologia Pediátrica",
                "Cirurgia Oftamológica",
                "Retina e Vítreo",
                "Glaucoma",
                "Córnea e doenças externas"
            ];
            SelectedSpecialty = Specialties[0];
            SelectedDoctorTitle = DoctorTitles[0];
            _remainingTime = TimeSpan.FromMinutes(15);

            IsLoading = false;
        }
        
        [RelayCommand] private void SetPatient() => SelectedUserType = (int)UserType.Patient;
        [RelayCommand] private void SetDoctor() => SelectedUserType = (int)UserType.Doctor;
        [RelayCommand] private void SetManager() => SelectedUserType = (int)UserType.Manager;

        [RelayCommand] private void SelectTab(int newtab)
        {
            SelectedTab = newtab;
            CanShowTabs = newtab == 0 || newtab == 1;
        }

        [RelayCommand]
        private void TogglepasswordVisibility()
        {
            IspasswordHidden = !IspasswordHidden;
        }

        [RelayCommand] private async Task RecoverPass()
        {
            try
            {
                await PopupHelper.PushLoadingAsync();

                if (!ValidationHelper.IsEmailValid(Email))
                    throw new Exception("Email inválido");

                var resp = await authService.RecoverUserPassword(Email);
                resp.ThrowIfIsNotSucess();

                string msg = resp.Response ?? "Ops... Parece que tivemos um problema ao tentar resetar a senha.\n\nTente novamente mais tarde.";
                await Messenger.ShowMessage(msg, "Informação");
            }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand]
        private async Task Login()
        {
            try
            {
                if (!ValidationHelper.IsEmailValid(Email))
                    throw new Exception("Email inválido");

                if (!ValidationHelper.IsPasswordValid(Password))
                    throw new Exception("Senha deve conter 6 caracteres no mínimo!");

                await PopupHelper.PushLoadingAsync();

                Master.AppMode = await authService.GetAppMode();

                var userTypeResp = await authService.GetUserTypeByEmail(Email);
                if (!userTypeResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();

                    string msg = userTypeResp.ResponseErr.Contains("Connection failure")
                        ? "Falha ao tentar se comunicar com nossos servidores.\n\nContate o administrador para mais detalhes"
                        : userTypeResp.ResponseErr;

                    throw new Exception(msg);
                }

                string? devicetoken = null;
#if ANDROID
                devicetoken = await FirebaseTokenService.GetTokenAsync();
#endif

                var userLoginResp = await authService.LoginAsync(Email, Password, (UserType)userTypeResp.Response, devicetoken);
                if (!userLoginResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();

                    string msg = userLoginResp.ResponseErr.Contains("Connection failure")
                        ? "Falha ao tentar se comunicar com nossos servidores.\n\nContate o administrador para mais detalhes"
                        : userLoginResp.ResponseErr;

                    throw new Exception(msg);
                }

                var user = userLoginResp.Response ?? throw new Exception("Ocorreu um erro interno.\nTente novamente mais tarde.");
                await PopupHelper.PopAllPopUpAsync();

                if (!CanUserLogin(user, out string message))
                {
                    if (user.Status == (int)UserStatus.PendingEmail)
                    {
                        await Messenger.ShowErrorMessageAsync("Por favor, confirme seu email para continuarmos","Confirmação Pendente");
                        registredUser = user;
                        await ConfirmEmail();
                        return;
                    }

                    string endmessage = user.Status == (int)UserStatus.PendingApproval ?
                        "\n\n⚡ Atenção: Fique atento à sua caixa de email! Enviaremos atualizações sobre seu cadastro diretamente lá!.\n\nAgradecemos a paciência. 😁"
                        : "\nPara mais informações contate o administrador.";

                    throw new Exception($"Falha ao tentar fazer o login...\n\nAtualmente seu cadastro se encontra no estado: \n\n🔺 {message.ToUpper()}\n\nPor isso não será possível entrar no aplicativo por enquanto.{endmessage}");
                }

                bool postPermission = await PermissionHelper.RequestPostNotificationsAsync();
                if (!postPermission)
                    throw new Exception("Por favor, aceite o envio de notificações para que possamos atualizá-lo em relação aos seus atendimentos.");

                this.SwitchToHomePage(user);
            }
            catch (OperationCanceledException)
            {
                Master.WasUnauthorized = false;
                Master.ResetGlobalToken();
            }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message, "Falha ao tentar fazer o login");
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        private static bool CanUserLogin(UserDto user, out string message)
        {
            if(user.Status == (int)UserStatus.Active)
            {
                message = "Sucessful";
                return true;
            }

            message = EnumTranslator.TranslateUserStatus(user.Status);
            return false;
        }

        private void SwitchToHomePage(UserDto? user)
        {
            UserHelper.SetLoggedUser(user);
            Master.ResetGlobalToken();

            var userType = (UserType?)user.Usertype;

            if (userType is null)
                return;

            switch (userType)
            {
                case UserType.Admin:
                    Navigator.SwitchPage(new AdminMainPage());
                    break;
                case UserType.Patient:
                    Navigator.SwitchPage(new PatientMainPage());
                    break;
                case UserType.Manager:
                    Navigator.SwitchPage(new ManagerMainPage());
                    break;
                case UserType.Doctor:
                    Navigator.SwitchPage(new DoctorMainPage());
                    break;
            }
        }

        partial void OnCepChanged(string value)
        {
            if (Cep.Length == 9)
                _ = SearchAdressByPostalCode();
        }

        [RelayCommand] private async Task SearchAdressByPostalCode()
        {
            try
            {
                await PopupHelper.PushLoadingAsync();
                var resp = await openCepService.FindAdressByCepAsync(Cep);
                if (!resp.WasSuccessful)
                    throw new Exception(resp.ResponseErr);

                var findedAdress = resp.Response;

                Adress = findedAdress.Logradouro ?? string.Empty;
                Neighborhood = findedAdress.Bairro ?? string.Empty;
                City = findedAdress.Localidade ?? string.Empty;
                State = findedAdress.Uf ?? string.Empty;
            }
            catch(Exception)
            {
                await PopupHelper.PopAllPopUpAsync();
                Master.ResetGlobalToken();
                return;
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand]
        public async Task Register()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Fullname))
                    throw new Exception("Informe o nome completo");

                if (!ValidationHelper.IsEmailValid(Email))
                    throw new Exception("Email inválido");

                if (!ValidationHelper.IsPhoneValid(Phone))
                    throw new Exception("Formato de telefone inválido (e.g. (11) 99999-9999)");

                if (!ValidationHelper.IsPasswordValid(Password) || Password != Confirmpassword)
                    throw new Exception("Senha não corresponde ou é muito curta");

                if (SelectedUserType == (int)UserType.Doctor && !ValidationHelper.IsNumber(MedicalExperience))
                    throw new Exception("Valor inválido informado para o tempo de experiência");

                if (SelectedUserType == (int)UserType.Doctor && !ValidationHelper.IsCRMValid(Crm))
                    throw new Exception("CRM é obrigatório");

                if (SelectedUserType == (int)UserType.Doctor &&
                    (string.IsNullOrEmpty(SelectedSpecialty) || SelectedSpecialty == Specialties[0]))
                    throw new Exception("Selecione a especialidade médica");

                if (!ValidationHelper.IsCPFValid(Cpf))
                    throw new Exception($"O CPF {Cpf} é inválido");

                if (SelectedUserType == (int)UserType.Patient && string.IsNullOrWhiteSpace(Adress))
                    throw new Exception("Endereço não informado!");

                if (SelectedUserType == (int)UserType.Patient && !ValidationHelper.IsValidCep(Cep))
                    throw new Exception($"O CEP {Cep} é inválido");

                if (SelectedUserType == (int)UserType.Patient && string.IsNullOrWhiteSpace(Neighborhood))
                    throw new Exception("Bairro não informado");

                if (SelectedUserType == (int)UserType.Patient && !ValidationHelper.IsNumber(Number))
                    throw new Exception("O número residencial é inválido");

                if (SelectedUserType == (int)UserType.Patient && string.IsNullOrEmpty(City))
                    throw new Exception("Cidade não informada");

                if (SelectedUserType == (int)UserType.Patient && !ValidationHelper.IsBrStateValid(State))
                    throw new Exception("O Estado informado é inválido");

                User user = new()
                {
                    Usertype = SelectedUserType,
                    Name = ValidationHelper.CapitalizeName(Fullname),
                    Email = Email.Trim(),
                    Fone = Phone,
                    Birthdate = DateOnly.FromDateTime(Birthdate),
                    Password = Password,
                    Ispremium = SelectedUserType == (int)UserType.Patient ? 0 : null,
                    Notifyemail = 1,
                    Notifypush = 1,
                    Status = (int)UserStatus.PendingEmail,
                    Cpf = Cpf,
                    Adress = Adress.Trim(),
                    Neighborhood = Neighborhood.Trim(),
                    Number = Number,
                    City = City.Trim(),
                    State = State.ToUpper().Trim(),
                    Postalcode = Cep.Trim(),
                    Complement = Complement.Trim()
                };

                await PopupHelper.PushLoadingAsync();

                DoctorProp? docProp = null;
                if(SelectedUserType == (int)UserType.Doctor)
                {
                    docProp = new()
                    {
                        Iddoctor = 0,
                        Title = SelectedDoctorTitle,
                        Crm = Crm,
                        Mainspecialty = SelectedSpecialty,
                        Medicalexperience = int.TryParse(MedicalExperience, out int result) ? result : 0,
                        Rating = 5,
                        Attendonline = 0,
                        Maxonlinedayconsultation = 0,
                        Maxpresencialdayconsultation = 0,
                    };
                }

                Master.AppMode = await authService.GetAppMode();

                var resp = await authService.RegisterUser(user, docProp);

                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception($"Falha ao cadastrar o usuário:\n{resp.ResponseErr}");
                }

                registredUser = resp.Response ?? throw new Exception("Falha ao tentar cadastrar usuário");

                await PopupHelper.PopAllPopUpAsync();
                await ConfirmEmail();
                
            }
            catch (OperationCanceledException)
            {
                Master.ResetGlobalToken();
            }
            catch (Exception ex)
            {
                Messenger.ShowErrorMessage($"Falha ao cadastrar:\n{ex.Message}");
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        public async Task<bool> IsPatient(UserDto baseUser)
        {
            if(baseUser.Usertype != (int)UserType.Patient)
            {
                await Messenger.ShowMessage(("Obrigado por se cadastrar!\nCadastros do tipo MÉDICO e GERENTE devem ser aprovados por um administrador. Assim que aprovado, você receberá uma confirmação no email e então poderá logar neste aplicativo 😊"), "Só falta mais um pouco...");
                Email = string.Empty;
                Password = string.Empty;
                Crm = string.Empty;
                Phone = string.Empty;
                Confirmpassword = string.Empty;
                Cpf = string.Empty;
                SelectedSpecialty = Specialties[0];
                Fullname = string.Empty;
                ValidationDigit1 = string.Empty;
                ValidationDigit2 = string.Empty;
                ValidationDigit3 = string.Empty;
                ValidationDigit4 = string.Empty;
                this.SelectTab(0);

                return false;
            }

            return true;
        }

        private async Task ConfirmEmail()
        {
            ResetTimer();
            await SendConfirmationEmail();
            if (emailSended)
            {
                this.SelectTab(2);
                StartTimerAsync();
            }
        }

        private async Task SendConfirmationEmail()
        {
            emailSended = false;

            try
            {
                await PopupHelper.PushLoadingAsync();

                if (string.IsNullOrWhiteSpace(Email))
                    throw new Exception("Insira um email válido");

                var resp = await authService.SendConfirmationEmail(Email);
                resp.ThrowIfIsNotSucess();

                emailSended = true;
            }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand] private async Task ReSendConfirmationEmail()
        {
            await SendConfirmationEmail();
            if (emailSended)
            {
                ResetTimer();
                StartTimerAsync();
            }
        }

        partial void OnValidationDigit4Changed(string value)
        {
            if (ValidationHelper.IsNumber(value))
                _ = ConfirmValidationCode();
        }

        private async Task ConfirmValidationCode()
        {
            if (registredUser is null)
                return;            

            try
            {
                await PopupHelper.PushLoadingAsync();

                StopTimer();

                string concatedCode = $"{ValidationDigit1}{ValidationDigit2}{ValidationDigit3}{ValidationDigit4}";
                int confirmcode = int.TryParse(concatedCode, out int result) ? result : 0;

                if (confirmcode < 1000)
                    throw new Exception($"O código informado {confirmcode} é inválido");

                var resp = await authService.ConfirmEmail(Email, confirmcode);
                resp.ThrowIfIsNotSucess();

                await Task.Delay(250);
                await PopupHelper.PopLoadingAsync();
                await Task.Delay(250);

                if (await IsPatient(registredUser))
                    this.SwitchToHomePage(registredUser);
            }
            catch(Exception ex)
            {
                ValidationDigit1 = string.Empty;
                ValidationDigit2 = string.Empty;
                ValidationDigit3 = string.Empty;
                ValidationDigit4 = string.Empty;
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopLoadingAsync();
        }

        private void StartTimerAsync()
        {
            if (_isRunningTimer)
                return;

            _isRunningTimer = true;
            _timerCts = new();
            var token = _timerCts.Token;

            _ = Task.Run(async () =>
            {
                while (_remainingTime.TotalSeconds > 0 && !token.IsCancellationRequested)
                {
                    await Task.Delay(1000, token);

                    if (token.IsCancellationRequested)
                        break;

                    _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));

                    MainThread.BeginInvokeOnMainThread(() => ValidationTimer = FormatTime(_remainingTime));
                }

                _isRunningTimer = false;
            }, token);
        }

        private void StopTimer()
        {
            if (!_isRunningTimer || _timerCts == null)
                return;

            _timerCts.Cancel();
            _isRunningTimer = false;
        }

        private void ResetTimer()
        {
            StopTimer();
            _remainingTime = TimeSpan.FromMinutes(15);
            ValidationTimer = FormatTime(_remainingTime);
        }

        private static string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }
    }
}
