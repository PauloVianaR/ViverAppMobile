using System.Net.Http.Json;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class AvailabilityClinicService : Service<AvailabilityClinic>
    {
        private const string endPoint = $"{baseApiPoint}/AvailabilityClinic";

        public AvailabilityClinicService() : base(endPoint) { }

        public override async Task<ResponseHandler<AvailabilityClinic>> CreateAsync(AvailabilityClinic entity)
        {
            ResponseHandler<AvailabilityClinic> resp = new();
            try
            {
                var httpResp = await HttpClient.PostAsJsonAsync(endPoint, new AvailabilityClinicDto
                {
                    Daytype = entity.Daytype,
                    Idclinic = entity.Idclinic,
                    Starttime = entity.Starttime,
                    Endtime = entity.Endtime,
                });
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<AvailabilityClinic>() ?? new();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public override async Task<ResponseHandler<bool>> UpdateAsync(int id, AvailabilityClinic entity)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.PutAsJsonAsync($"{endPoint}/{id}", new AvailabilityClinicDto
                {
                    Idavailabilityclinic = id,
                    Starttime = entity.Starttime,
                    Endtime = entity.Endtime,
                });
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
