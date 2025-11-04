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

namespace ViverAppMobile.ViewModels.Manager
{
    public partial class ManagerProfileViewModel : ObservableObject, IViewModelInstancer
    {
        private bool isLoading = false;
        private readonly AuthService authService;
        private readonly UserService userService;
        private UserDto? user;

        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private string fullName = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string cpf = string.Empty;
        [ObservableProperty] private bool notifyEmail = false;
        [ObservableProperty] private bool notifyPush = false;

        public ManagerProfileViewModel()
        {
            authService = new();
            userService = new();
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

                var userResp = await userService.GetByIdAsync(user.IdUser);
                if (!userResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    userResp.ThrowIfIsNotSucess();
                }

                user = userResp.Response;
                if (user is null)
                    throw new Exception("Falha ao tentar carregar seus dados de usuário");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    FullName = user.Name ?? string.Empty;
                    Email = user.Email ?? string.Empty;
                    Phone = user.Fone ?? string.Empty;
                    Cpf = user.Cpf ?? string.Empty;
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
            finally
            {
                isLoading = false;
            }
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
        private async Task SaveChanges()
        {
            if (user is null)
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
    }
}