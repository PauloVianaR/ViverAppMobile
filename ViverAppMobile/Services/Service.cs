using System.Buffers.Text;
using System.Net;
using System.Net.Http.Json;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class Service<T>(string endpoint) : BaseService where T : class, new()
    {
        private readonly string _endpoint = endpoint;

        public virtual async Task<ResponseHandler<IEnumerable<T>>> GetAllAsync(bool getAll = false)
        {
            ResponseHandler<IEnumerable<T>> resp = new();
            try
            {
                var httpResp = await HttpClient.GetAsync(_endpoint);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<T>>() ?? [];
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public virtual async Task<ResponseHandler<T>> GetByIdAsync(int id)
        {
            ResponseHandler<T> resp = new();
            try
            {
                var httpResp = await HttpClient.GetAsync($"{_endpoint}/{id}");
                if (!httpResp.IsSuccessStatusCode)
                {
                    if(httpResp.StatusCode == HttpStatusCode.NotFound)
                    {
                        resp.IsNotSuccess("Não Encontrado");
                        return resp;
                    }
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());
                }  

                var data = await httpResp.Content.ReadFromJsonAsync<T>() ?? new();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public virtual async Task<ResponseHandler<T>> CreateAsync(T entity)
        {
            ResponseHandler<T> resp = new();
            try
            {
                var httpResp = await HttpClient.PostAsJsonAsync(_endpoint, entity);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<T>() ?? new();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public virtual async Task<ResponseHandler<bool>> UpdateAsync(int id, T entity)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.PutAsJsonAsync($"{_endpoint}/{id}", entity);
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

        public virtual async Task<ResponseHandler<bool>> DeleteAsync(int id)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.DeleteAsync($"{_endpoint}/{id}");
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
