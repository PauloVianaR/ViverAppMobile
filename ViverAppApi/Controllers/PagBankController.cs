using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ViverApp.Shared.Context;
using ViverApp.Shared.Models;
using ViverApp.Shared.Models.PagBank;
using ViverAppApi.Models;

namespace ViverAppApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PagbankController : ControllerBase
    {
        private readonly HttpClient _http;
        private readonly ViverappmobileContext _context;

        public PagbankController(IHttpClientFactory httpClientFactory,ViverappmobileContext context,IOptions<PagBankSettings> pagbankOptions)
        {
            _context = context;
            var settings = pagbankOptions.Value;
            bool productionMode = _context.Configs.FirstOrDefault(c => c.Idconfig == (int)ConfigType.ProductionMode)?.Value == 1;

            _http = httpClientFactory.CreateClient();

            if (productionMode)
            {
                _http.BaseAddress = new Uri(settings.ProductionUrl!);
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", settings.TokenProduction);
            }
            else
            {
                _http.BaseAddress = new Uri(settings.SandboxUrl!);
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", settings.TokenSandbox);
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] CheckoutCreateRequest paymentRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(paymentRequest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync($"checkouts", content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return BadRequest(result);

                var checkout = JsonSerializer.Deserialize<CheckoutResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Ok(checkout);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromQuery] int idschedule, [FromBody] JsonElement body)
        {
            try
            {
                var orderId = body.GetProperty("id").GetString();
                if (string.IsNullOrEmpty(orderId))
                    return BadRequest("ID do pedido não fornecido");

                var resp = await _http.GetAsync($"orders/{orderId}");
                if (!resp.IsSuccessStatusCode)
                    return BadRequest("Não foi possível consultar o pedido");

                var respBody = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(respBody);
                var charge = doc.RootElement.GetProperty("charges")[0];
                var paymentMethod = charge.GetProperty("payment_method");
                var type = paymentMethod.GetProperty("type").GetString();

                int idpaymenttype = type switch
                {
                    "CREDIT_CARD" => 1,
                    "DEBIT_CARD" => 2,
                    "PIX" => 3,
                    "BOLETO" => 5,
                    _ => 0
                };

                string? cardLast4 = null;
                string? authorization = null;
                if (type == "CREDIT_CARD" || type == "DEBIT_CARD")
                {
                    if (paymentMethod.TryGetProperty("card", out var card))
                        cardLast4 = card.GetProperty("last4").GetString();
                    if (paymentMethod.TryGetProperty("authorization_code", out var auth))
                        authorization = auth.GetString();
                }

                var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Idschedule == idschedule);
                decimal paidprice = 0m;

                if (schedule is null)
                    return NotFound("O atendimento do pagamento não foi encontrado");

                var appnt = await _context.Appointments.FirstOrDefaultAsync(a => a.Idappointment == schedule.Idappointment);
                if (appnt is not null)
                {
                    paidprice = appnt.Price ?? 0m;
                }

                if (schedule.Status == (int)ScheduleStatus.Pending)
                    schedule.Status = (int)ScheduleStatus.Confirmed;

                schedule.Pendingpayment = 0;

                var payment = new Payment
                {
                    Idpaymenttype = idpaymenttype,
                    Idschedule = idschedule,
                    Paidday = DateTime.Now,
                    Paidprice = paidprice,
                    Paidonline = 1,
                    Cardlast4 = cardLast4,
                    Cardauthorization = authorization
                };

                _context.Payments.Add(payment);

                if (schedule.IddoctorNavigation?.DoctorProp is not null)
                {
                    var notification = new Notification
                    {
                        Notificationtype = (int)NotificationType.PaymentSuccess,
                        Severity = (int)Severity.Medium,
                        Title = "Pagamento de antedimento aprovado",
                        Description = $"Paciente: {schedule.IduserNavigation.Name}\nMédico: {schedule.IddoctorNavigation.Name}\nValor: {paidprice:c2}",
                        Pushdescription = $"{schedule.IduserNavigation.Name}, recebemos seu pagamento no valor de {paidprice:c2} referente ao seu atendimento com {schedule.IddoctorNavigation.DoctorProp.Title} {schedule.IddoctorNavigation.Name}. Seu atendimento foi agendado com sucesso! 😁",
                        Read = 0,
                        Sent = 0,
                        Targetid = schedule.Iduser
                    };

                    _context.Notifications.Add(notification);
                }                

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
