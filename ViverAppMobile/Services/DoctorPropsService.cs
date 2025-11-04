using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class DoctorPropsService : Service<DoctorProp>
    {
        private const string endPoint = $"{baseApiPoint}/DoctorProps";
        public DoctorPropsService() : base(endPoint) { }

        /// <summary>
        /// This <paramref name="id"/> must be the Iddoctor bound of DoctorProp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override async Task<ResponseHandler<DoctorProp>> GetByIdAsync(int id)
        {
            return await base.GetByIdAsync(id);
        }

        public override async Task<ResponseHandler<bool>> UpdateAsync(int id, DoctorProp entity)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.PutAsJsonAsync($"{endPoint}/{id}", new DoctorPropDto(entity));
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
