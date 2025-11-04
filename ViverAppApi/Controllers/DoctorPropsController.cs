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
    public class DoctorPropsController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public DoctorPropsController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorProp>>> GetDoctorProps()
        {
            return await _context.DoctorProps.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorProp>> GetDoctorProp(int id)
        {
            var doctorProp = await _context.DoctorProps.FirstOrDefaultAsync(dp => dp.Iddoctor == id);

            if (doctorProp is null)
            {
                return NotFound();
            }

            return doctorProp;
        }

        [HttpPost]
        public async Task<ActionResult<DoctorProp>> PostDoctorProp(DoctorProp doctorProp)
        {
            _context.DoctorProps.Add(doctorProp);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDoctorProp), new { id = doctorProp.Iddoctor }, doctorProp);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDoctorProp(int id, [FromBody] DoctorPropDto doctorProp)
        {
            if (id != doctorProp.Iddoctorprops)
            {
                return BadRequest();
            }

            var existingProps = await _context.DoctorProps.FirstOrDefaultAsync(x => x.Iddoctorprops == id);
            if (existingProps is null)
                return NotFound();

            existingProps.Crm = doctorProp.Crm;
            existingProps.Mainspecialty = doctorProp.Mainspecialty;
            existingProps.Medicalexperience = doctorProp.Medicalexperience;
            existingProps.Rating = doctorProp.Rating;
            existingProps.Attendonline = doctorProp.Attendonline;
            existingProps.Maxonlinedayconsultation = doctorProp.Maxonlinedayconsultation;
            existingProps.Maxpresencialdayconsultation = doctorProp.Maxpresencialdayconsultation;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DoctorPropExists(id))
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
        public async Task<IActionResult> DeleteDoctorProp(int id)
        {
            var doctorProp = await _context.DoctorProps.FindAsync(id);
            if (doctorProp == null)
            {
                return NotFound();
            }

            _context.DoctorProps.Remove(doctorProp);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DoctorPropExists(int id)
        {
            return _context.DoctorProps.Any(e => e.Iddoctorprops == id);
        }
    }
}
