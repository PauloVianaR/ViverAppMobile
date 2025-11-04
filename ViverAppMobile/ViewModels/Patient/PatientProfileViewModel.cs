using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Patient
{
    public partial class PatientProfileViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly UserService userService;
        private readonly AuthService authService;
        private readonly OpenCepService openCepService;
        private UserDto? user;

        [ObservableProperty] private bool userHasGooglePayConected = false;
        [ObservableProperty] private bool isUserPremium = false;
        [ObservableProperty] private string selectedTab = "1";
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string fullname = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string confirmpassword = string.Empty;
        [ObservableProperty] private string cpf = string.Empty;
        [ObservableProperty] private string cep = string.Empty;
        [ObservableProperty] private string adress = string.Empty;
        [ObservableProperty] private string neighborhood = string.Empty;
        [ObservableProperty] private string number = string.Empty;
        [ObservableProperty] private string complement = string.Empty;
        [ObservableProperty] private string city = string.Empty;
        [ObservableProperty] private string state = string.Empty;
        [ObservableProperty] private DateTime birthdate = new(2000, 1, 1);
        [ObservableProperty] private bool notifyEmail = false;
        [ObservableProperty] private bool notifyPush = false;

        public DateTime Today => DateTime.Today;
        public DateTime MinimumDate => new(1900, 1, 1);

        public PatientProfileViewModel()
        {
            userService = new();
            authService = new();
            openCepService = new();
        }

        public async Task InitializeAsync()
        {
            var loggedUser = UserHelper.GetLoggedUser();
            if (loggedUser is null)
                return;

            user = loggedUser;
            IsUserPremium = user.IsPremium == (sbyte)1;

            WeakReferenceMessenger.Default.Register<ShowProfilePageSelectTabByMainMessage>(this, (r, m) => SwitchSelectedTab(m.Value));

            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Email = user?.Email ?? string.Empty;
                    Fullname = user?.Name ?? string.Empty;
                    Phone = user?.Fone ?? string.Empty;
                    Cpf = user?.Cpf ?? string.Empty;
                    Cep = user?.Postalcode ?? string.Empty;
                    Adress = user?.Adress ?? string.Empty;
                    Neighborhood = user?.Neighborhood ?? string.Empty;
                    Number = user?.Number ?? string.Empty;
                    Complement = user?.Complement ?? string.Empty;
                    City = user?.City ?? string.Empty;
                    State = user?.State ?? string.Empty;
                    Birthdate = DateTime.TryParse(user?.BirthDate.ToString(), out DateTime bd) ? bd : new(2000, 1, 1);
                    NotifyEmail = (user.NotifyEmail ?? 0) == 1;
                    NotifyPush = (user.Notifypush ?? 0) == 1;
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
        }


        [RelayCommand] private void SwitchSelectedTab(string newTab) => SelectedTab = newTab;

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
                    throw new Exception(resp.ResponseErr);

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

        [RelayCommand] private async Task SaveUserChanges()
        {
            try
            {
                Master.GlobalToken.ThrowIfCancellationRequested();

                if (!ValidationHelper.IsCPFValid(Cpf))
                    throw new Exception($"O CPF {Cpf} é inváldio");

                if (!ValidationHelper.IsEmailValid(Email))
                    throw new Exception("O email informado é inválido");

                if (!ValidationHelper.IsPhoneValid(Phone))
                    throw new Exception("O telefone informado é inválido: O padrão deve ser: (31) 90000-0000");

                if (string.IsNullOrWhiteSpace(Adress))
                    throw new Exception("Informe o logradouro RUA/AVENIDA");

                if (!ValidationHelper.IsNumber(Number))
                    throw new Exception("O número residencial não pode conter letras ou estar vazio");

                if (!ValidationHelper.IsNotNumberOrEmpty(City))
                    throw new Exception("Cidade informada é inválida");

                if (!ValidationHelper.IsBrStateValid(State))
                    throw new Exception("Estado informado é inválido");

                await PopupHelper.PushLoadingAsync();

                user.Name = Fullname;
                user.Email = Email;
                user.Fone = Phone;
                user.Cpf = Cpf;
                user.Adress = Adress;
                user.Neighborhood = Neighborhood;
                user.Number = Number;
                user.City = City;
                user.State = State;
                user.Postalcode = Cep;
                user.Complement = Complement;
                user.BirthDate = DateOnly.FromDateTime(Birthdate);
                user.NotifyEmail = NotifyEmail ? (sbyte)1 : (sbyte)0;
                user.Notifypush = NotifyPush ? (sbyte)1 : (sbyte)0;

                var resp = await userService.UpdateAsync(user.IdUser, user);
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(resp.ResponseErr);
                }

                UserHelper.SetLoggedUser(user);

                UserHelper.GlobalUserChanged(true);
                await Messenger.ShowToastMessage("Alterações concluídas");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync($"Falha ao tentar atualizar os dados do usuário:\n{ex.Message}");
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand] private async Task SignPremium()
        {
#if DEBUG
            IsUserPremium = true;
#else
            await Messenger.ShowErrorMessageAsync("Ainda estamos trabalhando nesta funcionalidade.");
#endif
            await Task.CompletedTask;
        }
        [RelayCommand] private void UnsignPremium() => IsUserPremium = false;

        [RelayCommand] private async Task ChangePassword()
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
    }
}
