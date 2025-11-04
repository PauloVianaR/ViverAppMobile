using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp;
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
using ViverAppApi.Helpers;

namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public ScheduleController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedules()
        {
            return await _context.Schedules.ToListAsync();
        }

        [HttpGet("nextSchedules/{id}")]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetNextSchedule(int id,bool isDoctor = false,int page = 0,int pageSize = 10,int filterStatus = 0,string filterString = "",DateTime initialdate = default,DateTime finaldate = default, int modalityfilter = 0, int appointmenttypefilter = 0, int status1 = 1,int status2 = 2)
        {
            finaldate = finaldate == default ? DateTime.Today.AddYears(1) : finaldate;
            bool filtringString = !string.IsNullOrWhiteSpace(filterString);

            var normalizedFilter = filterString?.ToLower().Trim() ?? string.Empty;

            var query = _context.Schedules
                .Where(s => s.Status == status1 || s.Status == status2)
                .Where(s => s.Appointmentdate >= initialdate && s.Appointmentdate <= finaldate);

            if (id > 0)
            {
                if (isDoctor)
                    query = query.Where(s => s.Iddoctor == id);
                else
                    query = query.Where(s => s.Iduser == id);
            }

            if(filterStatus != 0 && filterStatus != 5)
            {
                query = query.Where(s => s.Status == filterStatus);
            }

            if(filterStatus == 5)
            {
                query = query.Where(s => s.Rescheduled == 1);
            }

            if (filtringString)
            {
                query = query.Where(s => !filtringString ||
                    (s.IddoctorNavigation!.Name!.ToLower().Trim().Contains(normalizedFilter)
                    || s.IdappointmentNavigation!.IdappointmenttypeNavigation!.Description!.ToLower().Trim().Contains(normalizedFilter)
                    || s.IdappointmentNavigation!.Title!.ToLower().Trim().Contains(normalizedFilter)
                    || s.IduserNavigation!.Name!.ToLower().Trim().Contains(normalizedFilter)));
            }
            
            if(modalityfilter > 0)
            {
                query = query.Where(s => s.Isonline + 1 == modalityfilter);
            }

            if(appointmenttypefilter > 0)
            {
                query = query.Where(s => s.IdappointmentNavigation.Idappointmenttype == appointmenttypefilter);
            }

            var filteredSchedules = await query
                .OrderByDescending(s => s.Idschedule)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(s => new ScheduleDto
                {
                    IdSchedule = s.Idschedule,
                    IdAppointment = s.Idappointment,
                    AppointmentType = s.IdappointmentNavigation.Idappointmenttype,
                    AppointmentTitle = s.IdappointmentNavigation.Title,
                    AppointmentDescription = s.IdappointmentNavigation.Description,
                    AppointmentPrice = s.IdappointmentNavigation.Price,
                    AverageTime = s.IdappointmentNavigation.Averagetime,

                    UserName = s.IduserNavigation.Name,
                    UserPhone = s.IduserNavigation.Fone,
                    PatientCpf = s.IduserNavigation.Cpf,
                    PatientEmail = s.IduserNavigation.Email,
                    PatientBirthDate = s.IduserNavigation.Birthdate,
                    IdPatient = s.Iduser,

                    Iddoctor = s.Iddoctor,
                    DoctorName = s.IddoctorNavigation.Name,
                    DoctorSpecialty = s.IddoctorNavigation.DoctorProp!.Mainspecialty,
                    DoctorTitle = s.IddoctorNavigation.DoctorProp!.Title,

                    ClinicFantasyName = s.IdclinicNavigation.Fantasyname,
                    ClinicAdress = s.IdclinicNavigation.Adress,
                    ClinicComplement = s.IdclinicNavigation.Complement,
                    ClinicNumber = s.IdclinicNavigation.Number,
                    ClinicCity = s.IdclinicNavigation.City,
                    ClinicNeighborhood = s.IdclinicNavigation.Neighborhood,
                    ClinicState = s.IdclinicNavigation.State,
                    ClinicPhone = s.IdclinicNavigation.Fone,
                    ClinicPostalCode = s.IdclinicNavigation.Postalcode,

                    Status = s.Status,
                    AppointmentDate = s.Appointmentdate,
                    Obs = s.Obs,
                    IsOnline = s.Isonline,
                    CallConcluded = s.Callconcluded,
                    Rescheduled = s.Rescheduled,
                    OriginalDate = s.Originaldate,
                    Rating = s.Rating,
                    MedicalReport = s.Medicalreport,
                    FeedBack = s.Feedback,
                    PendingPayment = s.Pendingpayment
                })
                .ToListAsync();

            return Ok(filteredSchedules);
        }


        [HttpGet("historic/{id}")]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetUserHistoric(int id, bool isDoctor = false,int page = 0, int pagesize = 10, int filterStatus = 0, string filterString = "", DateTime initialdate = default, DateTime finaldate = default, int modalityfilter = 0, int appointmenttypefilter = 0)
        {
            return await GetNextSchedule(id, isDoctor, page, pagesize, filterStatus, filterString, initialdate, finaldate, modalityfilter, appointmenttypefilter, 3, 4);
        }

        [HttpGet("count/{id}")]
        public async Task<ActionResult<int>> GetUserScheduleCount(int id, bool isDoctor = false,bool countingHistoric = false, int filterStatus = 0, string filterString = "", DateTime initialdate = default, DateTime finaldate = default)
        {
            finaldate = finaldate == default ? DateTime.Today.AddYears(1) : finaldate;
            bool filtringStatus = filterStatus != 0 && filterStatus != 5;
            bool filtringReschedules = filterStatus == 5;
            bool filtringString = filterString != string.Empty;
            int status1 = countingHistoric ? 3 : 1;
            int status2 = countingHistoric ? 4 : 2;
            var normalizedFilter = filterString?.ToLower().Trim() ?? string.Empty;

            var query = await _context.Schedules.Where(s => s.Status == status1 || s.Status == status2)
                .Where(s => !filtringStatus || s.Status == filterStatus)
                .Where(s => !filtringReschedules || s.Rescheduled == (sbyte)1)
                .Where(s => !filtringString || (s.IddoctorNavigation!.Name!.ToLower().Trim().Contains(normalizedFilter)! || s.IdappointmentNavigation!.IdappointmenttypeNavigation!.Description!.ToLower().Trim().Contains(normalizedFilter)))
                .Where(s => s.Appointmentdate >= initialdate && s.Appointmentdate <= finaldate)
                .ToListAsync();

            if(id > 0)
            {
                if (isDoctor)
                    query = query.Where(s => s.Iddoctor == id).ToList();
                else
                    query = query.Where(s => s.Iduser == id).ToList();
            }

            return Ok(query.Count);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Schedule>> GetSchedule(int id)
        {
            var schedule = await _context.Schedules
                .Include(a => a.IddoctorNavigation)
                .Include(a => a.IduserNavigation)
                .Include(a => a.IdclinicNavigation)
                .FirstOrDefaultAsync(a => a.Idschedule == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return schedule;
        }

        [HttpPost]
        public async Task<ActionResult<Schedule>> PostSchedule(ScheduleCreateDto dto)
        {
            var existingSchedule = _context.Schedules
                .FirstOrDefault(s => s.Iddoctor == dto.Iddoctor && s.Appointmentdate == dto.AppointmentDate);

            if (existingSchedule is not null)
                return BadRequest(new { message = "Já existe um agendamento para esse dia, horário e médico escolhidos" });

            var schedule = new Schedule
            {
                Iddoctor = dto.Iddoctor,
                Iduser = dto.Iduser,
                Idclinic = dto.Idclinic,
                Idappointment = dto.Idappointment,
                Appointmentdate = dto.AppointmentDate,
                Status = dto.Status,
                Obs = dto.Obs,
                Isonline = dto.IsOnline,
                Callconcluded = dto.IsOnline == 1 ? 0 : null,
                Rescheduled = dto.Rescheduled,
                Originaldate = dto.OriginalDate,
                Pendingpayment = dto.PendingPayment
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Idschedule }, schedule);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchSchedule(int id, ScheduleUpdateDto schedule)
        {
            if (id != schedule.Idschedule)
                return BadRequest();

            var existingSchedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Idschedule == id);
            if(existingSchedule is null)
                return NotFound("Não foi encontrado nenhum agendamento para ser atualizado");

            if (existingSchedule.Status != (int)ScheduleStatus.Canceled && schedule.Status == (int)ScheduleStatus.Canceled)
            {
                await this.SendUpdateScheduleEmail(schedule);
            }

            if (existingSchedule.Rescheduled != 0 && schedule.Rescheduled == 1)
            {
                await this.SendUpdateScheduleEmail(schedule);
            }

            existingSchedule.Pendingpayment = schedule.PendingPayment;
            existingSchedule.Status = schedule.Status;
            existingSchedule.Appointmentdate = schedule.Appointmentdate;
            existingSchedule.Rescheduled = schedule.Rescheduled;
            existingSchedule.Rating = schedule.Rating;
            existingSchedule.Medicalreport = schedule.MedicalReport;
            existingSchedule.Feedback = schedule.Feedback;
            existingSchedule.Callconcluded = schedule.CallConcluded;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
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

        private async Task SendUpdateScheduleEmail(ScheduleUpdateDto schedule)
        {
            var pacient = await _context.Users.FirstOrDefaultAsync(u => u.Iduser == schedule.IdPacient);
            if (pacient is null)
                return;

            if (pacient.Notifyemail == 0)
                return;

            string emailTo = pacient.Email ?? string.Empty;
            if (string.IsNullOrWhiteSpace(emailTo))
                return;

            if (pacient.Usertype == schedule.UserTypeUpdated)
                return;

            EmailSender.Context = _context;

            if (schedule.Status == (int)ScheduleStatus.Canceled)
                await SendCancellationEmail(schedule, emailTo);
            else if(schedule.Rescheduled == 1)
                await SendRescheduleEmail(schedule, emailTo);
        }

        private static async Task SendCancellationEmail(ScheduleUpdateDto schedule, string emailTo)
        {
            string cancelEmailBody = $@"
            <html>
              <head>
                <style>
                  body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f6f8;
                    margin: 0;
                    padding: 0;
                  }}
                  .container {{
                    max-width: 600px;
                    margin: 30px auto;
                    background: #ffffff;
                    padding: 25px 35px;
                    border-radius: 8px;
                    box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                  }}
                  h2 {{
                    color: #e74c3c;
                    text-align: center;
                  }}
                  .status-box {{
                    margin: 25px 0;
                    padding: 18px;
                    text-align: center;
                    font-size: 20px;
                    font-weight: bold;
                    color: #ffffff;
                    background: #e74c3c;
                    border-radius: 8px;
                    letter-spacing: 2px;
                  }}
                  p {{
                    font-size: 14px;
                    color: #333;
                    line-height: 1.6;
                  }}
                  .highlight {{
                    background-color: #fdecea;
                    border-left: 4px solid #e74c3c;
                    padding: 10px 15px;
                    margin: 15px 0;
                    border-radius: 6px;
                    color: #c0392b;
                    font-style: italic;
                  }}
                  .footer {{
                    margin-top: 30px;
                    font-size: 12px;
                    color: #777;
                    text-align: center;
                  }}
                </style>
              </head>
              <body>
                <div class='container'>
                  <h2>Atendimento Cancelado 😔</h2>
                  <p>Olá, {schedule.PacientName}.</p>

                  <div class='status-box'>O atendimento <b>{schedule.AppointmentTitle}</b> foi cancelado.</div>

                  <p>
                    O atendimento que estava agendado com o(a) Dr(a). <b>{schedule.DoctorName}</b> foi cancelado.
                  </p>

                  <div class='highlight'>
                    <b>Motivo do cancelamento:</b><br>
                    {schedule.Feedback}
                  </div>

                  <p>
                    Caso já tenha realizado o pagamento, fique tranquilo(a): o valor será <b>reembolsado integralmente</b> no mesmo método utilizado.
                  </p>

                  <p>
                    Se preferir, você pode reagendar um novo horário diretamente pelo aplicativo do <b>Viver App</b>.
                  </p>

                  <div class='footer'>
                    &copy; {DateTime.Now.Year} Viver App — Todos os direitos reservados
                  </div>
                </div>
              </body>
            </html>";

            await EmailSender.SendEmailAsync(emailTo, "Cancelamento de Atendimento", cancelEmailBody, Severity.Medium);
        }

        private static async Task SendRescheduleEmail(ScheduleUpdateDto schedule, string emailTo)
        {
            string rescheduleEmailBody = $@"
            <html>
              <head>
                <style>
                  body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f6f8;
                    margin: 0;
                    padding: 0;
                  }}
                  .container {{
                    max-width: 600px;
                    margin: 30px auto;
                    background: #ffffff;
                    padding: 25px 35px;
                    border-radius: 8px;
                    box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                  }}
                  h2 {{
                    color: #2980b9;
                    text-align: center;
                  }}
                  .status-box {{
                    margin: 25px 0;
                    padding: 18px;
                    text-align: center;
                    font-size: 20px;
                    font-weight: bold;
                    color: #ffffff;
                    background: #3498db;
                    border-radius: 8px;
                    letter-spacing: 2px;
                  }}
                  p {{
                    font-size: 14px;
                    color: #333;
                    line-height: 1.6;
                  }}
                  .highlight {{
                    background-color: #ebf5fb;
                    border-left: 4px solid #3498db;
                    padding: 10px 15px;
                    margin: 15px 0;
                    border-radius: 6px;
                    color: #21618c;
                  }}
                  .footer {{
                    margin-top: 30px;
                    font-size: 12px;
                    color: #777;
                    text-align: center;
                  }}
                </style>
              </head>
              <body>
                <div class='container'>
                  <h2>Atendimento Reagendado 📅</h2>
                  <p>Olá, {schedule.PacientName}.</p>

                  <div class='status-box'>Seu atendimento foi reagendado!</div>

                  <p>
                    O atendimento <b>{schedule.AppointmentTitle}</b> com o(a) Dr(a). <b>{schedule.DoctorName}</b> foi reagendado com sucesso.
                  </p>

                  <div class='highlight'>
                    <b>Data original:</b> {schedule.OriginalDate:dd/MM/yyyy HH:mm}<br>
                    <b>Nova data:</b> {schedule.Appointmentdate:dd/MM/yyyy HH:mm}
                  </div>

                  <p>
                    Fique à vontade para ajustar sua agenda conforme a nova data e horário.
                    Qualquer dúvida, estamos aqui pra ajudar 💙
                  </p>

                  <div class='footer'>
                    &copy; {DateTime.Now.Year} Viver App — Todos os direitos reservados
                  </div>
                </div>
              </body>
            </html>";

            await EmailSender.SendEmailAsync(emailTo, "Reagendamento de Atendimento", rescheduleEmailBody, Severity.Medium);
        }

        [HttpPatch("updatemany")]
        public async Task<IActionResult> PatchSchedules(IEnumerable<ScheduleUpdateDto> schedules)
        {
            foreach (var schedule in schedules)
            {
                var existingSchedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Idschedule == schedule.Idschedule);
                if (existingSchedule is null)
                    continue;

                if (existingSchedule.Status != (int)ScheduleStatus.Canceled && schedule.Status == (int)ScheduleStatus.Canceled)
                {
                    await this.SendUpdateScheduleEmail(schedule);
                }

                if (existingSchedule.Rescheduled != 0 && schedule.Rescheduled == 1)
                {
                    await this.SendUpdateScheduleEmail(schedule);
                }

                existingSchedule.Pendingpayment = schedule.PendingPayment;
                existingSchedule.Status = schedule.Status;
                existingSchedule.Appointmentdate = schedule.Appointmentdate;
                existingSchedule.Rescheduled = schedule.Rescheduled;
                existingSchedule.Rating = schedule.Rating;
                existingSchedule.Medicalreport = schedule.MedicalReport;
                existingSchedule.Feedback = schedule.Feedback;
                existingSchedule.Callconcluded = schedule.CallConcluded;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchedule(int id, Schedule schedule)
        {
            if (id != schedule.Idschedule)
            {
                return BadRequest();
            }

            _context.Entry(schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
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
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.Idschedule == id);
        }
    }
}