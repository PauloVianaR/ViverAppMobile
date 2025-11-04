using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ViverApp.Shared.Models;
using ViverApp.Shared.Context;


namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ClinicController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public ClinicController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Clinic>>> GetClinics()
        {
            return await _context.Clinics.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Clinic>> GetClinic(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);

            if (clinic == null)
            {
                return NotFound();
            }

            return clinic;
        }

        [HttpPost]
        public async Task<ActionResult<Clinic>> PostClinic(Clinic clinic)
        {
            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClinic), new { id = clinic.Idclinic }, clinic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutClinic(int id, Clinic clinic)
        {
            if (id != clinic.Idclinic)
            {
                return BadRequest();
            }

            _context.Entry(clinic).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicExists(id))
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
        public async Task<IActionResult> DeleteClinic(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }

            _context.Clinics.Remove(clinic);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClinicExists(int id)
        {
            return _context.Clinics.Any(e => e.Idclinic == id);
        }
    }
}