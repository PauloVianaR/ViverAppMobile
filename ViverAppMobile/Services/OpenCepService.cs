using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class OpenCepService() : BaseService("https://opencep.com/v1/")
    {
        public async Task<ResponseHandler<AdressResponseOpenCepDto?>> FindAdressByCepAsync(string cep)
        {
            ResponseHandler<AdressResponseOpenCepDto?> resp = new();
            try
            {
                if (string.IsNullOrWhiteSpace(cep))
                    throw new Exception("CEP inválido");

                cep = new string(cep.Where(char.IsDigit).ToArray());

                if (cep.Length != 8)
                    throw new Exception("CEP inválido");

                var httpResp = await HttpClient.GetAsync(cep);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var adress = await httpResp.Content.ReadFromJsonAsync<AdressResponseOpenCepDto>();
                resp.IsSuccess(adress);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess($"Erro ao consultar CEP: {ex.Message}");
            }

            return resp;
        }
    }
}
