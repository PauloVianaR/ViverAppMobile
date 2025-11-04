using System.Net.Http.Json;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class SpecialtyDoctorService : Service<SpecialtysDoctor>
    {
        public const string endPoint = $"{baseApiPoint}/SpecialtysDoctor";

        public SpecialtyDoctorService() : base(endPoint) { }

        public async Task<ResponseHandler<IEnumerable<SpecialtysDoctor>>> GetAllByDoctorAsync(int iddoctor)
        {
            ResponseHandler<IEnumerable<SpecialtysDoctor>> resp = new();
            try
            {
                var httpResp = await HttpClient.GetAsync($"{endPoint}/{iddoctor}");
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<SpecialtysDoctor>>() ?? [];
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public override async Task<ResponseHandler<SpecialtysDoctor>> CreateAsync(SpecialtysDoctor entity)
        {
            ResponseHandler<SpecialtysDoctor> resp = new();
            try
            {
                var httpResp = await HttpClient.PostAsJsonAsync(endPoint, new SpecialtyDoctorDto(entity));
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<SpecialtysDoctor>() ?? new();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }
    }
}
