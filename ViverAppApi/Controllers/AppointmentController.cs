using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverApp.Shared.Context;

namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public AppointmentController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointment()
        {
            return await _context.Appointments
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Idappointment == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return appointment;
        }

        [HttpGet("existsinschedule/{id}")]
        public async Task<ActionResult<bool>> AppointmentExistsInSchedule(int id)
        {
            return await _context.Schedules.AnyAsync(s => s.Idappointment == id);
        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> PostAppointment(AppointmentDto appointmentCreate)
        {
            var appntType = await _context.AppointmentTypes.FirstOrDefaultAsync(a => a.Idappointmenttype == appointmentCreate.Idappointmenttype);
            var newAppnt = new Appointment
            {
                Idappointmenttype = appointmentCreate.Idappointmenttype,
                Title = appointmentCreate.Title,
                Description = appointmentCreate.Description,
                Averagetime = appointmentCreate.Averagetime,
                Price = appointmentCreate.Price,
                Ispopular = appointmentCreate.Ispopular,
                Canonline = appointmentCreate.Canonline,
                Status = appointmentCreate.Status,
                IdappointmenttypeNavigation = appntType ?? new AppointmentType()
            };

            _context.Appointments.Add(newAppnt);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppointment), new { id = newAppnt.Idappointment }, newAppnt);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchAppointment(int id, AppointmentDto appointment)
        {
            if (id != appointment.Idappointment)
            {
                return BadRequest();
            }

            var appnt = await _context.Appointments.FirstOrDefaultAsync(a => a.Idappointment == id);
            if (appnt is null)
                return NotFound();

            appnt.Idappointmenttype = appointment.Idappointmenttype;
            appnt.Status = appointment.Status;
            appnt.Ispopular = appointment.Ispopular;
            appnt.Averagetime = appointment.Averagetime;
            appnt.Title = appointment.Title;
            appnt.Description = appointment.Description;
            appnt.Price = appointment.Price;
            appnt.Canonline = appointment.Canonline;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id))
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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointment(int id, Appointment appointment)
        {
            if (id != appointment.Idappointment)
            {
                return BadRequest();
            }

            _context.Entry(appointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id))
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
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Idappointment == id);
        }
    }
}