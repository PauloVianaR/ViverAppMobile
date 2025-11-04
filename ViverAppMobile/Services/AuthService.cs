using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ViverApp.Shared.Dtos;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Handlers;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Services
{
    public class AuthService : BaseService
    {
        private const string endPoint = $"{baseApiPoint}/Auth";

        public async Task<AppMode> GetAppMode()
        {
            try
            {
                var resp = await HttpClient.GetAsync($"{endPoint}/getAppMode");
                if (resp.IsSuccessStatusCode)
                {
                    var isProduction = await resp.Content.ReadFromJsonAsync<bool>();
                    if(isProduction)
                        return AppMode.Production;
                }
            }
            catch (Exception) { }

            return AppMode.Homologation;
        }

        public async Task<ResponseHandler<UserDto>> RegisterUser(User user,DoctorProp? doctorProp = null)
        {
            ResponseHandler<UserDto> resp = new();

            try
            {
                var request = new RegisterRequestDto(user, new DoctorPropDto(doctorProp));
                var response = await HttpClient.PostAsJsonAsync($"{endPoint}/register", request);

                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    throw new Exception(msg);
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("access_token", out var at) &&
                    root.TryGetProperty("refresh_token", out var rt)
                    && user.Usertype == (int)UserType.Patient)
                {
                    var access = at.GetString();
                    var refresh = rt.GetString();
                    if (!string.IsNullOrEmpty(access) && !string.IsNullOrEmpty(refresh))
                        await AuthSession.SaveTokensAsync(access, refresh, user.Usertype);
                }

                UserDto? dto = null;
                if (root.TryGetProperty("user", out var userProp))
                    dto = JsonSerializer.Deserialize<UserDto>(userProp.GetRawText(), _jsonOptions);

                resp.IsSuccess(dto ?? Activator.CreateInstance<UserDto>());

                if (user.Name is null)
                    return resp;
                
                if(user.Usertype == (int)UserType.Manager || user.Usertype == (int)UserType.Doctor)
                {
                    NotificationType notificationType;
                    string description;

                    notificationType = NotificationType.AwaitingApproval;
                    description = $"{user.Name}\nTipo: {EnumTranslator.TranslateUserType(user.Usertype)}\nTelefone: {user.Fone}";
                    description += doctorProp is null ? $"\nCPF: {user.Cpf}" : $"\nCRM: {doctorProp.Crm}";

                    Notificator.Send(notificationType, description);
                }
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }


        public async Task<ResponseHandler<UserDto>> LoginAsync(string email, string password, UserType type, string? devicetoken)
        {
            var loginRequest = new LoginRequestDto
            {
                Email = email.ToLower().Trim(),
                Password = password,
                UserType = (int)type,
                Devicetoken = devicetoken
            };

            ResponseHandler<UserDto> resp = new();

            try
            {
                var response = await HttpClient.PostAsJsonAsync($"{endPoint}/login", loginRequest);

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("access_token", out var at) &&
                    root.TryGetProperty("refresh_token", out var rt))
                {
                    var access = at.GetString();
                    var refresh = rt.GetString();
                    if (!string.IsNullOrEmpty(access) && !string.IsNullOrEmpty(refresh))
                        await AuthSession.SaveTokensAsync(access, refresh, loginRequest.UserType);
                }

                UserDto? user = default;
                if (root.TryGetProperty("user", out var userProp))
                {
                    user = JsonSerializer.Deserialize<UserDto>(userProp.GetRawText(), _jsonOptions);
                }

                resp.IsSuccess(user ?? Activator.CreateInstance<UserDto>());
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }


        public async Task<ResponseHandler<bool>> ChangeUserPassword(UserDto? user, string oldpass, string newpass)
        {
            var request = new ChangePasswordRequestDto
            {
                Id = user.IdUser,
                UserType = user.Usertype,
                OldPassword = oldpass,
                NewPassword = newpass
            };

            ResponseHandler<bool> resp = new();
            try
            {
                if (request.Id == 0)
                    throw new Exception("Falha ao alterar a senha, contate o administrador.");

                var response = await HttpClient.PatchAsJsonAsync($"{endPoint}/changePassword", request);

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                await AuthSession.ClearAsync(user, clearDeviceToken: false);

                resp.IsSuccess(true);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        public async Task<ResponseHandler<string>> RecoverUserPassword(string email)
        {
            ResponseHandler<string> resp = new();
            try
            {
                string normalizedEmail = email.ToLower().Trim();

                var response = await HttpClient.PostAsJsonAsync($"{endPoint}/resetPassword", normalizedEmail);
                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                var data = await response.Content.ReadAsStringAsync();

                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public async Task<ResponseHandler<int>> GetUserTypeByEmail(string email)
        {
            ResponseHandler<int> resp = new();
            try
            {
                var response = await HttpClient.GetAsync($"{endPoint}/getUserTypeByEmail?email={email}");
                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                var data = await response.Content.ReadFromJsonAsync<int>();

                resp.IsSuccess(data);
            }
            catch(Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        public async Task<ResponseHandler<bool>> SendConfirmationEmail(string email)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var response = await HttpClient.PostAsJsonAsync($"{endPoint}/sendConfirmationEmail", email);
                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                resp.IsSuccess(true);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public async Task<ResponseHandler<bool>> ConfirmEmail(string email, int confirmationcode)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var response = await HttpClient.PostAsJsonAsync($"{endPoint}/confirmemail", new EmailValidationRequestDto()
                {
                    Email = email,
                    ConfirmationCode = confirmationcode
                });
                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                resp.IsSuccess(true);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        public async Task DeleteTokensAsync(int id, bool clearDeviceToken)
        {
            try
            {
                _ = await HttpClient.DeleteAsync($"{endPoint}/clearTokens/{id}?clearDeviceToken={clearDeviceToken}");
            }
            catch { }
        }
    }
}
