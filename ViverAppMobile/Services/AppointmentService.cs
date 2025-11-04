using System.Net;
using System.Net.Http.Json;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class AppointmentService : Service<Appointment>
    {
        public const string endPoint = $"{baseApiPoint}/Appointment";

        public AppointmentService() : base(endPoint) { }

        public override async Task<ResponseHandler<Appointment>> CreateAsync(Appointment entity)
        {
            ResponseHandler<Appointment> resp = new();
            try
            {
                var httpResp = await HttpClient.PostAsJsonAsync(endPoint, new AppointmentDto
                {
                    Idappointmenttype = entity.Idappointmenttype,
                    Title = entity.Title,
                    Description = entity.Description,
                    Averagetime = entity.Averagetime,
                    Price = entity.Price,
                    Ispopular = entity.Ispopular,
                    Canonline = entity.Canonline,
                    Status = entity.Status
                });
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<Appointment>() ?? new();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public override async Task<ResponseHandler<bool>> UpdateAsync(int id, Appointment entity)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.PatchAsJsonAsync($"{endPoint}/{id}", new AppointmentDto
                {
                    Idappointment = entity.Idappointment,
                    Idappointmenttype = entity.Idappointmenttype,
                    Title = entity.Title,
                    Description = entity.Description,
                    Averagetime = entity.Averagetime,
                    Price = entity.Price,
                    Ispopular = entity.Ispopular,
                    Canonline = entity.Canonline,
                    Status = entity.Status
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

        public async Task<ResponseHandler<bool>> ExistsInSchedule(int id)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.GetAsync($"{endPoint}/existsinschedule/{id}");
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<bool>();
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
