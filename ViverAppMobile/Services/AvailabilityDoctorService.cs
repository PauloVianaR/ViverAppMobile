using System.Net.Http.Json;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class AvailabilityDoctorService : Service<AvailabilityDoctor>
    {
        public const string endPoint = $"{baseApiPoint}/AvailabilityDoctor";
        public AvailabilityDoctorService() : base(endPoint) { }

        public async Task<ResponseHandler<IEnumerable<AvailabilityDoctor>>> GetAllAsync(int iddoc)
        {
            ResponseHandler<IEnumerable<AvailabilityDoctor>> resp = new();
            try
            {
                var httpResp = await HttpClient.GetAsync($"{endPoint}/byDoctor/{iddoc}");
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<AvailabilityDoctor>>() ?? [];
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public override async Task<ResponseHandler<AvailabilityDoctor>> CreateAsync(AvailabilityDoctor entity)
        {
            ResponseHandler<AvailabilityDoctor> resp = new();
            try
            {
                var httpResp = await HttpClient.PostAsJsonAsync(endPoint, new AvailabilityDoctorDto(entity));
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<AvailabilityDoctor>() ?? new();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public override async Task<ResponseHandler<bool>> UpdateAsync(int id, AvailabilityDoctor entity)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.PutAsJsonAsync($"{endPoint}/{id}", new AvailabilityDoctorDto(entity));
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                resp.IsSuccess(true);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public async Task<ResponseHandler<bool>> CreateDefaultAsync(int iddoctor)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.PostAsJsonAsync($"{endPoint}/createdefault", iddoctor);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                resp.IsSuccess(true);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }
    }
}
