using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverApp.Shared.Context;

namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SpecialtysDoctorController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public SpecialtysDoctorController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpecialtysDoctor>>> GetSpecialtysDoctors()
        {
            return await _context.SpecialtysDoctors
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IEnumerable<SpecialtysDoctor>> GetSpecialtysDoctor(int id)
        {
            var specialtysDoctor = await _context.SpecialtysDoctors
                .Where(m => m.Iddoctor == id)
                .ToListAsync();

            return specialtysDoctor;
        }

        [HttpPost]
        public async Task<ActionResult<SpecialtysDoctor>> PostSpecialtysDoctor(SpecialtyDoctorDto dto)
        {
            bool existsSD = await _context.SpecialtysDoctors.AnyAsync(sd => sd.Iddoctor == dto.Iddoctor && sd.Idappointment == dto.Idappointment);
            if (existsSD)
                return BadRequest("Este tipo de serviço já está vinculado ao(à) médico(a)");

            SpecialtysDoctor specialtysDoctor = new()
            {
                Idspecialtysdoctor = 0,
                Iddoctor = dto.Iddoctor,
                Idappointment = dto.Idappointment
            };

            _context.SpecialtysDoctors.Add(specialtysDoctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpecialtysDoctor), new { id = specialtysDoctor.Idspecialtysdoctor }, specialtysDoctor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpecialtysDoctor(int id, SpecialtysDoctor specialtysDoctor)
        {
            if (id != specialtysDoctor.Idspecialtysdoctor)
            {
                return BadRequest();
            }

            _context.Entry(specialtysDoctor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpecialtysDoctorExists(id))
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
        public async Task<IActionResult> DeleteSpecialtysDoctor(int id)
        {
            var specialtysDoctor = await _context.SpecialtysDoctors.FindAsync(id);
            if (specialtysDoctor == null)
            {
                return NotFound();
            }

            _context.SpecialtysDoctors.Remove(specialtysDoctor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpecialtysDoctorExists(int id)
        {
            return _context.SpecialtysDoctors.Any(e => e.Idspecialtysdoctor == id);
        }
    }
}
