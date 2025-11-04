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
    public class AvailabilityClinicController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public AvailabilityClinicController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AvailabilityClinic>>> GetAvailabilityClinics()
        {
            return await _context.AvailabilityClinics.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AvailabilityClinic>> GetAvailabilityClinic(int id)
        {
            var availabilityClinic = await _context.AvailabilityClinics
                .FirstOrDefaultAsync(m => m.Idavailabilityclinic == id);

            if (availabilityClinic == null)
            {
                return NotFound();
            }

            return availabilityClinic;
        }

        [HttpPost]
        public async Task<ActionResult<AvailabilityClinic>> PostAvailabilityClinic(AvailabilityClinicDto availabilityClinic)
        {
            AvailabilityClinic newAc = new()
            {
                Idclinic = availabilityClinic.Idclinic,
                Daytype = availabilityClinic.Daytype,
                Starttime = availabilityClinic.Starttime,
                Endtime = availabilityClinic.Endtime,
            };

            _context.AvailabilityClinics.Add(newAc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAvailabilityClinic), new { id = newAc.Idavailabilityclinic }, newAc);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAvailabilityClinic(int id, AvailabilityClinicDto availabilityClinic)
        {
            if (id != availabilityClinic.Idavailabilityclinic)
            {
                return BadRequest();
            }

            var existingAc = await _context.AvailabilityClinics.FirstOrDefaultAsync(a => a.Idavailabilityclinic == id);
            if (existingAc is null)
                return NotFound();

            existingAc.Starttime = availabilityClinic.Starttime;
            existingAc.Endtime = availabilityClinic.Endtime;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AvailabilityClinicExists(id))
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
        public async Task<IActionResult> DeleteAvailabilityClinic(int id)
        {
            var availabilityClinic = await _context.AvailabilityClinics.FindAsync(id);
            if (availabilityClinic == null)
            {
                return NotFound();
            }

            _context.AvailabilityClinics.Remove(availabilityClinic);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AvailabilityClinicExists(int id)
        {
            return _context.AvailabilityClinics.Any(e => e.Idavailabilityclinic == id);
        }
    }
}
