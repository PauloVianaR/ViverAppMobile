using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverApp.Shared.Utils;
using ViverApp.Shared.Context;

namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public PaymentController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            return await _context.Payments
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Idpayment == id);

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        [HttpGet("userPayments/{id}")]
        public async Task<ActionResult<IEnumerable<PaymentHistoricDto>>> GetUserPayments(int id, int page = 0, int pageSize = 10, DateTime initialDate = default, DateTime finalDate = default, decimal minValue = 0m, decimal maxValue = 0m, int paymentType = 0, int wherePaid = 0)
        {
            finalDate = finalDate == default ? DateTimeHelper.GetTodayLastTime() : finalDate.AddDays(1);

            var query = await _context.Payments
                .Include(p => p.IdpaymenttypeNavigation)
                .Include(p => p.IdscheduleNavigation).ThenInclude(s => s.IdappointmentNavigation).ThenInclude(a => a.IdappointmenttypeNavigation)
                .Include(p => p.IdscheduleNavigation).ThenInclude(s => s.IddoctorNavigation).ThenInclude(a => a.DoctorProp)
                .Where(p => p.IdscheduleNavigation.Iduser == id)
                .ToListAsync();

            query = query.Where(p => p.Paidday >= initialDate && p.Paidday <= finalDate).ToList();

            if (minValue > 0 && maxValue == 0)
            {
                query = query.Where(p => p.Paidprice >= minValue).ToList();
            }
            else if (maxValue > 0)
            {
                query = query.Where(p => p.Paidprice >= minValue && p.Paidprice <= maxValue).ToList();
            }

            if (paymentType == 1)
            {
                query = query.Where(p => p.Idpaymenttype == (int)PayMethod.CREDIT_CARD || p.Idpaymenttype == (int)PayMethod.DEBIT_CARD).ToList();
            }
            else if (paymentType > 1)
            {
                query = query.Where(p => p.Idpaymenttype == paymentType + 1).ToList();
            }

            if (wherePaid == (int)WherePaidPayment.Online)
            {
                query = query.Where(p => p.Paidonline == (sbyte)1).ToList();
            }
            else if (wherePaid == (int)WherePaidPayment.Presencial)
            {
                query = query.Where(p => p.Paidonline == (sbyte)0).ToList();
            }

            var result = query
                .OrderByDescending(p => p.Idpayment)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentHistoricDto
                {
                    Idpayment = p.Idpayment,
                    Idpaymenttype = p.Idpaymenttype,
                    PaymentDescription = p.IdpaymenttypeNavigation.Description,
                    AppointmentTitle = p.IdscheduleNavigation.IdappointmentNavigation.Title,
                    AppointmentDescription = p.IdscheduleNavigation.IdappointmentNavigation.Description,
                    ScheduleDate = p.IdscheduleNavigation.Appointmentdate,
                    Paidday = p.Paidday,
                    Paidprice = p.Paidprice,
                    Paidonline = p.Paidonline,
                    ProfessionalDoctorName = $"{p.IdscheduleNavigation.IddoctorNavigation.DoctorProp!.Title} {p.IdscheduleNavigation.IddoctorNavigation.Name}"
                }).ToList();

            return result;
        }

        [HttpGet("userPayments/count/{id}")]
        public async Task<ActionResult<int>> GetUserPaymentsCount(int id, DateTime initialDate = default, DateTime finalDate = default, decimal minValue = 0m, decimal maxValue = 0m, int paymentType = 0, int wherePaid = 0)
        {
            finalDate = finalDate == default ? DateTimeHelper.GetTodayLastTime() : finalDate.AddDays(1);

            var query = await _context.Payments
                .Include(p => p.IdpaymenttypeNavigation)
                .Include(p => p.IdscheduleNavigation).ThenInclude(s => s.IdappointmentNavigation)
                .Include(p => p.IdscheduleNavigation).ThenInclude(s => s.IddoctorNavigation)
                .Where(p => p.IdscheduleNavigation.Iduser == id).ToListAsync();

            query = query.Where(p => p.Paidday >= initialDate && p.Paidday <= finalDate).ToList();

            if (minValue > 0 && maxValue == 0)
            {
                query = query.Where(p => p.Paidprice >= minValue).ToList();
            }
            else if (maxValue > 0)
            {
                query = query.Where(p => p.Paidprice >= minValue && p.Paidprice <= maxValue).ToList();
            }

            if (paymentType == 1)
            {
                query = query.Where(p => p.Idpaymenttype == (int)PayMethod.CREDIT_CARD || p.Idpaymenttype == (int)PayMethod.DEBIT_CARD).ToList();
            }
            else if (paymentType > 1)
            {
                query = query.Where(p => p.Idpaymenttype == paymentType + 1).ToList();
            }

            if (wherePaid == (int)WherePaidPayment.Online)
            {
                query = query.Where(p => p.Paidonline == (sbyte)1).ToList();
            }
            else if (wherePaid == (int)WherePaidPayment.Presencial)
            {
                query = query.Where(p => p.Paidonline == (sbyte)0).ToList();
            }

            return query.Count;
        }

        [HttpGet("paymentsByMonths/{months}")]
        public async Task<ActionResult<IEnumerable<PaymentHistoricDto>>> GetPaymentsByMonths(int months = 1, DateTime startDate = default)
        {
            startDate = startDate == default ? DateTime.Today.AddDays(1) : startDate;
            var endDate = startDate;
            var beginDate = DateTimeHelper.GetFirstDayThisMonth().AddMonths(-months + (months == 1 ? 1 : 0));

            var query = _context.Payments
                .Include(p => p.IdpaymenttypeNavigation)
                .Include(p => p.IdscheduleNavigation).ThenInclude(s => s.IdappointmentNavigation).ThenInclude(s => s.IdappointmenttypeNavigation)
                .Include(p => p.IdscheduleNavigation).ThenInclude(s => s.IddoctorNavigation)
                .Include(p => p.IdscheduleNavigation).ThenInclude(s => s.IduserNavigation)
                .Where(p => p.Paidday >= beginDate && p.Paidday <= endDate);

            var result = await query
                .Select(p => new PaymentHistoricDto
                {
                    Idpayment = p.Idpayment,
                    Idpaymenttype = p.Idpaymenttype,
                    PaymentDescription = p.IdpaymenttypeNavigation.Description,
                    AppointmentTitle = p.IdscheduleNavigation.IdappointmentNavigation.Title,
                    AppointmentDescription = p.IdscheduleNavigation.IdappointmentNavigation.Description,
                    AppointmentTypeDescription = p.IdscheduleNavigation.IdappointmentNavigation.IdappointmenttypeNavigation.Description,
                    ScheduleDate = p.IdscheduleNavigation.Appointmentdate,
                    Paidday = p.Paidday,
                    Paidprice = p.Paidprice,
                    Paidonline = p.Paidonline,
                    ProfessionalDoctorName = p.IdscheduleNavigation.IddoctorNavigation.Name,
                    IsUserPremium = p.IdscheduleNavigation.IduserNavigation.Ispremium
                })
                .OrderByDescending(p => p.Paidday)
                .ToListAsync();

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> PostPayment(PaymentCreateDto payment)
        {
            var schedule = _context.Schedules.FirstOrDefault(s => s.Idschedule == payment.Idschedule);
            if (schedule is null)
                return NotFound("Não foi encontrado o agendamento referente para dar a baixa no pagamento");

            schedule.Pendingpayment = 0;

            var newpayment = new Payment()
            {
                Idpaymenttype = payment.Idpaymenttype,
                Idschedule = payment.Idschedule,
                Paidday = payment.Paidday,
                Paidprice = payment.Paidprice,
                Paidonline = payment.Paidonline,
                Cardlast4 = payment.Cardlast4,
                Cardauthorization = payment.Cardauthorization,
            };

            _context.Payments.Add(newpayment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayment), new { id = newpayment.Idpayment }, newpayment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayment(int id, Payment payment)
        {
            if (id != payment.Idpayment)
            {
                return BadRequest();
            }

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Idpayment == id);
        }
    }
}
