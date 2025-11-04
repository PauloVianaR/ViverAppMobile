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
    public class HolidayController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public HolidayController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Holiday>>> GetHolidays()
        {
            return await _context.Holidays.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Holiday>> GetHoliday(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);

            if (holiday == null)
            {
                return NotFound();
            }

            return holiday;
        }

        [HttpPost]
        public async Task<ActionResult<Holiday>> PostHoliday(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHoliday), new { id = holiday.Idholiday }, holiday);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutHoliday(int id, Holiday holiday)
        {
            if (id != holiday.Idholiday)
            {
                return BadRequest();
            }

            _context.Entry(holiday).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HolidayExists(id))
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
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null)
            {
                return NotFound();
            }

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HolidayExists(int id)
        {
            return _context.Holidays.Any(e => e.Idholiday == id);
        }
    }
}
