using System.Net.Http.Json;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverApp.Shared.Utils;
using ViverAppMobile.Handlers;

namespace ViverAppMobile.Services
{
    public class PaymentService : Service<Payment>
    {
        public const string endPoint = $"{baseApiPoint}/Payment";
        public PaymentService() : base(endPoint) { }

        public async Task<ResponseHandler<IEnumerable<PaymentHistoricDto>>> GetPaymentsByUser(UserDto user, int page = 0, int pageSize = 10, DateTime initialDate = default, DateTime finalDate = default, decimal minValue = 0m, decimal maxValue = 0m, int paymentType = 0, int wherePaid = 0)
        {
            if(finalDate == default)
            {
                finalDate = DateTime.Today;
            }

            ResponseHandler<IEnumerable<PaymentHistoricDto>> resp = new();
            try
            {
                string uri = $"{endPoint}/userPayments/{user.IdUser}?page={page}&pageSize={pageSize}&initialDate={initialDate:yyyy-MM-dd}&finalDate={finalDate:yyyy-MM-dd}&minValue={minValue}&maxValue={maxValue}&paymentType={paymentType}&wherePaid={wherePaid}";

                var httpResp = await HttpClient.GetAsync(uri);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<PaymentHistoricDto>>() ?? [];
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public async Task<ResponseHandler<int>> GetPaymentsByUserCounting(UserDto user, DateTime initialDate = default, DateTime finalDate = default, decimal minValue = 0m, decimal maxValue = 0m, int paymentType = 0, int wherePaid = 0)
        {
            if (finalDate == default)
            {
                finalDate = DateTime.Today;
            }

            ResponseHandler<int> resp = new();
            try
            {
                string uri = $"{endPoint}/userPayments/count/{user.IdUser}?initialDate={initialDate:yyyy-MM-dd}&finalDate={finalDate:yyyy-MM-dd}&minValue={minValue}&maxValue={maxValue}&paymentType={paymentType}&wherePaid={wherePaid}";

                var httpResp = await HttpClient.GetAsync(uri);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<int>();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public async Task<ResponseHandler<IEnumerable<PaymentHistoricDto>>> GetPaymentsByMonths(int months = 1, DateTime startDate = default, bool isMonthsBefore = false)
        {
            ResponseHandler<IEnumerable<PaymentHistoricDto>> resp = new();
            try
            {
                if (isMonthsBefore)
                {
                    startDate = startDate == default ? DateTime.Today.AddDays(1) : startDate;

                    var date = startDate.AddMonths(-months + (months == 1 ? 0 : 1));
                    startDate = DateTimeHelper.GetLastDayOfMonth(date.Year, date.Month);
                }

                var httpResp = await HttpClient.GetAsync($"{endPoint}/paymentsByMonths/{months}?startDate={startDate:yyyy-MM-dd}");
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<PaymentHistoricDto>>() ?? [];
                resp.IsSuccess(data);
            }
            catch(Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        public override async Task<ResponseHandler<Payment>> CreateAsync(Payment entity)
        {
            ResponseHandler<Payment> resp = new();
            try
            {
                var httpResp = await HttpClient.PostAsJsonAsync(endPoint, new PaymentCreateDto(entity));
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<Payment>() ?? new();
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
