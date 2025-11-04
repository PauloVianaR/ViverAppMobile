using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models.PagBank;
using ViverAppMobile.Handlers;
using ViverAppMobile.Helpers;
using ViverAppMobile.Workers;
using static System.Net.WebRequestMethods;

namespace ViverAppMobile.Services
{
    public class PagBankService : BaseService
    {
        private const string endPoint = $"{baseApiPoint}/Pagbank";

        public async Task<ResponseHandler<string?>> GetCheckoutLinkAsync(ScheduleDto schedule)
        {
            ResponseHandler<string?> response = new();
            try
            {
                string pagBankWebhook = Master.GetUrl(UrlType.PagBankWebhook);
                if (string.IsNullOrWhiteSpace(pagBankWebhook))
                    throw new Exception("Falha ao tentar obter o webhook de pagamento.\nTente novamente");

                string pagBankRedirectUrl = Master.GetUrl(UrlType.PagBankRedirect);
                if (string.IsNullOrWhiteSpace(pagBankRedirectUrl))
                    throw new Exception("Falha ao tentar obter a url de redirecionamento de pagamento.\nTente novamente");

                var allPhone = schedule.UserPhone.Split(") ");

                string areaCode = allPhone[0].Replace("(", string.Empty).Trim();
                string rawPhone = allPhone[1].Replace("-", string.Empty).Trim();

                string appntType = EnumTranslator.TranslateAppointmentType(schedule.AppointmentType);

                var order = new CheckoutCreateRequest
                {
                    Reference_id = $"PED-{schedule.IdSchedule}",
                    Expiration_date = DateTime.Now.AddMinutes(15),
                    Customer = new CustomerRequest
                    {
                        Name = schedule.UserName,
                        Email = schedule.PatientEmail,
                        Phone = new()
                        {
                            Country = "55",
                            Area = areaCode,
                            Number = rawPhone,
                        },
                        Tax_id = new string(schedule.PatientCpf?.Where(char.IsDigit).ToArray())
                    },
                    Items =
                    [
                        new ItemRequest
                        {
                            Name = $"Pagamento de {appntType} (Codigo do Atendimento:{schedule.IdSchedule})",
                            Description = $"Tipo de {appntType.ToUpper()}: {schedule.AppointmentTitle}",
                            Quantity = 1,
                            Unit_amount = (int)((schedule.AppointmentPrice ?? 1) * 100),
                            Reference_id = schedule.IdSchedule.ToString()
                        }
                    ],
                    Payment_methods =
                    [
                        new PaymentMethodRequest { Type = "CREDIT_CARD" },
                        new PaymentMethodRequest { Type = "DEBIT_CARD" },
                        new PaymentMethodRequest { Type = "PIX" },
                        new PaymentMethodRequest { Type = "BOLETO" }
                    ],
                    Redirect_url = pagBankRedirectUrl,
                    Return_url = pagBankRedirectUrl,
                    Notification_urls =
                    [
                        $"{pagBankWebhook}/{endPoint}/webhook?idschedule={schedule.IdSchedule}"
                    ],
                    Soft_descriptor = $"ViverAppMobile"
                };

                var resp = await HttpClient.PostAsJsonAsync($"{endPoint}/checkout", order);
                if (!resp.IsSuccessStatusCode)
                {
                    var respData = await resp.Content.ReadAsStringAsync();
                    throw new Exception($"Falha ao tentar fazer o pagamento.\nDados:\n{respData}");
                }

                var json = await resp.Content.ReadFromJsonAsync<CheckoutResponse>();
                response.IsSuccess(json?.Links?.FirstOrDefault(r => r.Rel == "PAY")?.Href);
            }
            catch (Exception ex)
            {
                response.IsNotSuccess(ex.Message);
            }
            return response;
        }
    }
}
