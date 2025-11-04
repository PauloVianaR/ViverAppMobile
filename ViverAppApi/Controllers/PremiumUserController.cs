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
    public class PremiumUserController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public PremiumUserController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PremiumUser>>> GetPremiumUsers()
        {
            return await _context.PremiumUsers
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PremiumUser>> GetPremiumUser(int id)
        {
            var premiumUser = await _context.PremiumUsers
                .FirstOrDefaultAsync(m => m.IdpremiumUser == id);

            if (premiumUser == null)
            {
                return NotFound();
            }

            return premiumUser;
        }

        [HttpPost]
        public async Task<ActionResult<PremiumUser>> PostPremiumUser(PremiumUser premiumUser)
        {
            _context.PremiumUsers.Add(premiumUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPremiumUser), new { id = premiumUser.IdpremiumUser }, premiumUser);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPremiumUser(int id, PremiumUser premiumUser)
        {
            if (id != premiumUser.IdpremiumUser)
            {
                return BadRequest();
            }

            _context.Entry(premiumUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PremiumUserExists(id))
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
        public async Task<IActionResult> DeletePremiumUser(int id)
        {
            var premiumUser = await _context.PremiumUsers.FindAsync(id);
            if (premiumUser == null)
            {
                return NotFound();
            }

            _context.PremiumUsers.Remove(premiumUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PremiumUserExists(int id)
        {
            return _context.PremiumUsers.Any(e => e.IdpremiumUser == id);
        }
    }
}
