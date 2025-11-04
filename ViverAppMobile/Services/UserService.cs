using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ViverApp.Shared.Dtos;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class UserService : Service<UserDto>
    {
        private const string endPoint = $"{baseApiPoint}/User";
        public UserService() : base(endPoint) { }

        public override async Task<ResponseHandler<IEnumerable<UserDto>>> GetAllAsync(bool getAll = false)
        {
            return await this.GetAllAsync(getBlocked: getAll, getRejected: getAll, getPendingApproval: getAll);
        }

        public async Task<ResponseHandler<IEnumerable<UserDto>>> GetAllAsync(bool getBlocked, bool getRejected, bool getPendingApproval, 
            UserType usertype = UserType.None)
        {
            ResponseHandler<IEnumerable<UserDto>> resp = new();
            try
            {
                string uri = $"{endPoint}?getBlocked={getBlocked}&getRejected={getRejected}&getPendingApproval={getPendingApproval}&usertype={(int)usertype}";

                var httpResp = await HttpClient.GetAsync(uri);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<UserDto>>() ?? [];
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public async Task<ResponseHandler<IEnumerable<DoctorDto>>> GetDoctorsAsync(
            bool getBlocked = false, 
            bool getRejected = false, 
            bool getPendingApproval = false)
        {
            ResponseHandler<IEnumerable<DoctorDto>> resp = new();
            try
            {
                string uri = $"{endPoint}/doctors?getBlocked={getBlocked}&getRejected={getRejected}&getPendingApproval={getPendingApproval}";

                var httpResp = await HttpClient.GetAsync(uri);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<DoctorDto>>() ?? [];
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public async Task<ResponseHandler<DoctorDto>> GetDoctorAsync(int id)
        {
            ResponseHandler<DoctorDto> resp = new();
            try
            {
                var httpResp = await HttpClient.GetAsync($"{endPoint}/doctor/{id}");
                if (!httpResp.IsSuccessStatusCode)
                {
                    if (httpResp.StatusCode == HttpStatusCode.NotFound)
                    {
                        resp.IsNotSuccess("Não Encontrado");
                        return resp;
                    }
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());
                }

                var data = await httpResp.Content.ReadFromJsonAsync<DoctorDto>() ?? new();
                resp.IsSuccess(data);
            }
            catch(Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }
    }
}
