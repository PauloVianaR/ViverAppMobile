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
    public class AvailabilityDoctorController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public AvailabilityDoctorController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AvailabilityDoctor>>> GetAvailabilityDoctors()
        {
            return await _context.AvailabilityDoctors
                .Include(a => a.IddoctorNavigation)
                .Where(a => a.IddoctorNavigation.Status == (int)UserStatus.Active)
                .ToListAsync();
        }

        [HttpGet("byDoctor/{id}")]
        public async Task<ActionResult<IEnumerable<AvailabilityDoctor>>> GetAvailabilityDoctorsByDoc(int id)
        {
            return await _context.AvailabilityDoctors.Where(a => a.Iddoctor == id).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AvailabilityDoctor>> GetAvailabilityDoctor(int id)
        {
            var availabilityDoctor = await _context.AvailabilityDoctors
                .FirstOrDefaultAsync(m => m.Idavailabilitydoctor == id);

            if (availabilityDoctor == null)
            {
                return NotFound();
            }

            return availabilityDoctor;
        }

        [HttpPost]
        public async Task<ActionResult<AvailabilityDoctor>> PostAvailabilityDoctor(AvailabilityDoctorDto availabilityDoctor)
        {
            AvailabilityDoctor newAvDoc = new(availabilityDoctor);

            _context.AvailabilityDoctors.Add(newAvDoc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAvailabilityDoctor), new { id = newAvDoc.Idavailabilitydoctor }, newAvDoc);
        }

        [HttpPost("createdefault")]
        public async Task<IActionResult> PostAvailabilityDoctorCreateDefault([FromBody] int iddoctor)
        {
            try
            {
                AvailabilityDoctor monday = new(0,iddoctor, daytype: (int)DayOfWeek.Monday, starttime: new TimeOnly(8, 0), endtime: new TimeOnly(17, 0), isonline: 0);
                AvailabilityDoctor tuesday = new(0, iddoctor, daytype: (int)DayOfWeek.Tuesday, starttime: new TimeOnly(8, 0), endtime: new TimeOnly(17, 0), isonline: 0);
                AvailabilityDoctor wednesday = new(0, iddoctor, daytype: (int)DayOfWeek.Wednesday, starttime: new TimeOnly(8, 0), endtime: new TimeOnly(17, 0), isonline: 0);
                AvailabilityDoctor thursday = new(0, iddoctor, daytype: (int)DayOfWeek.Thursday, starttime: new TimeOnly(8, 0), endtime: new TimeOnly(17, 0), isonline: 0);
                AvailabilityDoctor friday = new(0, iddoctor, daytype: (int)DayOfWeek.Friday, starttime: new TimeOnly(8, 0), endtime: new TimeOnly(17, 0), isonline: 0);

                await _context.AvailabilityDoctors.AddRangeAsync(monday, tuesday, wednesday, thursday, friday);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAvailabilityDoctor(int id, AvailabilityDoctorDto availabilityDoctor)
        {
            if (id != availabilityDoctor.Idavailabilitydoctor)
            {
                return BadRequest();
            }

            var existingAd = await _context.AvailabilityDoctors.FirstOrDefaultAsync(ad => ad.Idavailabilitydoctor == id);
            if (existingAd is null)
                return NotFound();

            existingAd.Starttime = availabilityDoctor.Starttime;
            existingAd.Endtime = availabilityDoctor.Endtime;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AvailabilityDoctorExists(id))
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
        public async Task<IActionResult> DeleteAvailabilityDoctor(int id)
        {
            var availabilityDoctor = await _context.AvailabilityDoctors.FindAsync(id);
            if (availabilityDoctor == null)
            {
                return NotFound();
            }

            _context.AvailabilityDoctors.Remove(availabilityDoctor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AvailabilityDoctorExists(int id)
        {
            return _context.AvailabilityDoctors.Any(e => e.Idavailabilitydoctor == id);
        }
    }
}
