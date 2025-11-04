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
    public class PremiumPlanController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public PremiumPlanController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PremiumPlan>>> GetPremiumPlans()
        {
            return await _context.PremiumPlans.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PremiumPlan>> GetPremiumPlan(int id)
        {
            var premiumPlan = await _context.PremiumPlans.FindAsync(id);

            if (premiumPlan == null)
            {
                return NotFound();
            }

            return premiumPlan;
        }

        [HttpPost]
        public async Task<ActionResult<PremiumPlan>> PostPremiumPlan(PremiumPlan premiumPlan)
        {
            _context.PremiumPlans.Add(premiumPlan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPremiumPlan), new { id = premiumPlan.Idpremiumplan }, premiumPlan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPremiumPlan(int id, PremiumPlan premiumPlan)
        {
            if (id != premiumPlan.Idpremiumplan)
            {
                return BadRequest();
            }

            _context.Entry(premiumPlan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PremiumPlanExists(id))
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
        public async Task<IActionResult> DeletePremiumPlan(int id)
        {
            var premiumPlan = await _context.PremiumPlans.FindAsync(id);
            if (premiumPlan == null)
            {
                return NotFound();
            }

            _context.PremiumPlans.Remove(premiumPlan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PremiumPlanExists(int id)
        {
            return _context.PremiumPlans.Any(e => e.Idpremiumplan == id);
        }
    }
}
